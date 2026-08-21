using Content.Server.Popups;
using Content.Server.UserInterface;
using Content.Shared.Access.Systems;
using Content.Shared.DeadSpace.PatrolTablet;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusIcon;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.PatrolTablet;

public sealed class PatrolTabletSystem : EntitySystem
{
    private const int MaxSquads = 16;
    private const int MaxSquadNameLength = 30;
    private const string SquadIconPrototypePrefix = "DeadSpaceSquadIcon";

    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PatrolTabletComponent, AfterActivatableUIOpenEvent>(OnUiOpen);
        SubscribeLocalEvent<PatrolTabletComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<PatrolTabletComponent, PatrolTabletRenameSquadMessage>(OnRenameSquad);
        SubscribeLocalEvent<PatrolTabletComponent, PatrolTabletBulkAssignSquadMessage>(OnBulkAssignSquad);
        SubscribeLocalEvent<PatrolTabletComponent, PatrolTabletClearAllMessage>(OnClearAll);
        SubscribeLocalEvent<PatrolTabletComponent, PatrolTabletClearSquadMessage>(OnClearSquad);
        SubscribeLocalEvent<PatrolTabletComponent, PatrolTabletCreateSquadMessage>(OnCreateSquad);
        SubscribeLocalEvent<PatrolTabletComponent, PatrolTabletDeleteSquadMessage>(OnDeleteSquad);
    }

    private bool AddTrackedPersonnel(EntityUid uid, PatrolTabletComponent comp, EntityUid target, EntityUid user)
    {
        if (!_idCard.TryFindIdCard(target, out _))
        {
            _popup.PopupEntity(Loc.GetString("patrol-tablet-no-id-card"), uid, user);
            return false;
        }

        var netTarget = GetNetEntity(target);

        if (comp.TrackedPersonnel.Contains(netTarget))
            return false;

        if (!HasComp<PatrolMemberComponent>(target))
            AddComp<PatrolMemberComponent>(target);

        comp.TrackedPersonnel.Add(netTarget);
        Dirty(uid, comp);

        _popup.PopupEntity(Loc.GetString("patrol-tablet-added-personnel", ("name", MetaData(target).EntityName)), uid, user);
        return true;
    }

    private void OnAfterInteract(EntityUid uid, PatrolTabletComponent comp, AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (!HasComp<MobStateComponent>(args.Target.Value))
            return;

        if (comp.TrackedPersonnel.Contains(GetNetEntity(args.Target.Value)))
        {
            _popup.PopupEntity(Loc.GetString("patrol-tablet-already-tracked"), uid, args.User);
            return;
        }

        if (AddTrackedPersonnel(uid, comp, args.Target.Value, args.User))
            UpdateUiState(uid, comp);
        args.Handled = true;
    }

    private void OnInteractUsing(InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<PatrolTabletComponent>(args.Used, out var comp))
            return;

        if (!HasComp<MobStateComponent>(args.Target))
            return;

        if (comp.TrackedPersonnel.Contains(GetNetEntity(args.Target)))
        {
            _popup.PopupEntity(Loc.GetString("patrol-tablet-already-tracked"), args.Used, args.User);
            args.Handled = true;
            return;
        }

        if (AddTrackedPersonnel(args.Used, comp, args.Target, args.User))
            UpdateUiState(args.Used, comp);
        args.Handled = true;
    }

    private void OnUiOpen(EntityUid uid, PatrolTabletComponent comp, AfterActivatableUIOpenEvent args)
    {
        UpdateUiState(uid, comp);
    }

    private static string? SanitizeSquadName(string name)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return name.Length > MaxSquadNameLength
            ? name[..MaxSquadNameLength]
            : name;
    }

    private bool IsValidSquadIcon(string iconId)
    {
        return iconId.StartsWith(SquadIconPrototypePrefix, StringComparison.Ordinal)
               && _prototype.TryIndex<SecurityIconPrototype>(iconId, out var icon)
               && !icon.Abstract;
    }

    private void OnRenameSquad(EntityUid uid, PatrolTabletComponent comp, PatrolTabletRenameSquadMessage msg)
    {
        var name = SanitizeSquadName(msg.NewName);
        if (name == null)
            return;

        var squad = comp.Squads.Find(s => s.Id == msg.SquadId);
        if (squad == null)
            return;

        squad.Name = name;
        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void OnBulkAssignSquad(EntityUid uid, PatrolTabletComponent comp, PatrolTabletBulkAssignSquadMessage msg)
    {
        var squad = comp.Squads.Find(s => s.Id == msg.SquadId);
        if (squad == null)
            return;

        foreach (var netEntity in comp.TrackedPersonnel)
        {
            var target = GetEntity(netEntity);
            if (!Exists(target))
                continue;

            SetSquadOnIdCard(target, squad.Id, squad.IconId);
        }

        comp.TrackedPersonnel.Clear();
        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void OnClearAll(EntityUid uid, PatrolTabletComponent comp, PatrolTabletClearAllMessage msg)
    {
        comp.TrackedPersonnel.Clear();
        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void OnClearSquad(EntityUid uid, PatrolTabletComponent comp, PatrolTabletClearSquadMessage msg)
    {
        var query = EntityQueryEnumerator<PatrolSquadCardComponent>();
        while (query.MoveNext(out var cardUid, out var squadCard))
        {
            if (squadCard.SquadId != msg.SquadId)
                continue;

            squadCard.SquadId = string.Empty;
            squadCard.SquadIcon = string.Empty;
            squadCard.MemberName = string.Empty;
            Dirty(cardUid, squadCard);
        }

        UpdateUiState(uid, comp);
    }

    private void OnCreateSquad(EntityUid uid, PatrolTabletComponent comp, PatrolTabletCreateSquadMessage msg)
    {
        var name = SanitizeSquadName(msg.Name);
        if (name == null || !IsValidSquadIcon(msg.IconId) || comp.Squads.Count >= MaxSquads)
            return;

        var id = $"squad_{Guid.NewGuid():N}"[..16];
        comp.Squads.Add(new SquadData(id, name, msg.IconId));
        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void OnDeleteSquad(EntityUid uid, PatrolTabletComponent comp, PatrolTabletDeleteSquadMessage msg)
    {
        var squad = comp.Squads.Find(s => s.Id == msg.SquadId);
        if (squad == null)
            return;

        comp.Squads.Remove(squad);

        var query = EntityQueryEnumerator<PatrolSquadCardComponent>();
        while (query.MoveNext(out var cardUid, out var squadCard))
        {
            if (squadCard.SquadId != msg.SquadId)
                continue;

            squadCard.SquadId = string.Empty;
            squadCard.SquadIcon = string.Empty;
            squadCard.MemberName = string.Empty;
            Dirty(cardUid, squadCard);
        }

        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void SetSquadOnIdCard(EntityUid target, string squadId, string squadIcon)
    {
        if (!_idCard.TryFindIdCard(target, out var idCard))
            return;

        var name = idCard.Comp.FullName ?? MetaData(target).EntityName;

        var squadCard = EnsureComp<PatrolSquadCardComponent>(idCard.Owner);
        squadCard.SquadId = squadId;
        squadCard.SquadIcon = squadIcon;
        squadCard.MemberName = name;
        Dirty(idCard.Owner, squadCard);
    }

    private void UpdateUiState(EntityUid uid, PatrolTabletComponent comp)
    {
        if (!_ui.HasUi(uid, PatrolTabletUiKey.Key))
            return;

        var officers = new List<PatrolOfficerInfo>();
        var squads = GetSquads(comp);

        foreach (var netEntity in comp.TrackedPersonnel)
        {
            var target = GetEntity(netEntity);
            if (!Exists(target))
                continue;

            var name = "Unknown";
            var jobTitle = "Security";

            if (TryComp(target, out MetaDataComponent? meta))
            {
                name = meta.EntityName;
            }

            var squadId = string.Empty;
            var squadIcon = string.Empty;

            if (_idCard.TryFindIdCard(target, out var idCard))
            {
                if (idCard.Comp.FullName != null)
                    name = idCard.Comp.FullName;
                jobTitle = idCard.Comp.LocalizedJobTitle ?? "Security";

                if (TryComp<PatrolSquadCardComponent>(idCard.Owner, out var squadCard))
                {
                    squadId = squadCard.SquadId;
                    squadIcon = squadCard.SquadIcon;
                }
            }

            var info = new PatrolOfficerInfo(
                GetNetEntity(target).ToString(),
                name,
                jobTitle)
            {
                SquadId = squadId,
                SquadIcon = squadIcon,
            };

            officers.Add(info);
        }

        var squadMemberNames = new Dictionary<string, HashSet<string>>();

        var cardQuery = EntityQueryEnumerator<PatrolSquadCardComponent>();
        while (cardQuery.MoveNext(out var cardUid, out var squadCard))
        {
            if (string.IsNullOrEmpty(squadCard.SquadId) || string.IsNullOrEmpty(squadCard.MemberName))
                continue;

            if (!squadMemberNames.TryGetValue(squadCard.SquadId, out var memberSet))
            {
                memberSet = new HashSet<string>();
                squadMemberNames[squadCard.SquadId] = memberSet;
            }

            memberSet.Add(squadCard.MemberName);
        }

        foreach (var squad in squads)
        {
            if (squadMemberNames.TryGetValue(squad.SquadId, out var members))
            {
                squad.AssignedCount = members.Count;
                squad.Members.AddRange(members);
            }
        }

        _ui.SetUiState(uid, PatrolTabletUiKey.Key,
            new PatrolTabletUpdateState(officers, squads));
    }

    private List<PatrolSquadDef> GetSquads(PatrolTabletComponent comp)
    {
        var result = new List<PatrolSquadDef>();
        foreach (var squad in comp.Squads)
        {
            result.Add(new PatrolSquadDef(squad.Id, squad.Name, squad.IconId));
        }
        return result;
    }
}
