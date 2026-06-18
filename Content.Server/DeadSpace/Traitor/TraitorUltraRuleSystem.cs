// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.Backmen.Economy;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Server.Traitor.Uplink;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Backmen.Economy;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Chat;
using Content.Shared.Corvax.TTS;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.DeadSpace.Traitor;
using Content.Shared.Dataset;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Roles.Jobs;
using Content.Shared.Store.Components;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.DeadSpace.Traitor;

public sealed class TraitorUltraRuleSystem : GameRuleSystem<TraitorUltraRuleComponent>
{
    private const int SourceParentSearchDepth = 8;

    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly BankManagerSystem _bankManager = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ISharedChatManager _chatManager = default!;
    [Dependency] private readonly SharedCargoSystem _cargo = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly SharedObjectivesSystem _sharedObjectives = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;
    [Dependency] private readonly UplinkSystem _uplink = default!;

    private readonly List<TraitorUltraDelayedAction> _delayedActions = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TraitorUltraRuleComponent, AfterAntagEntitySelectedEvent>(OnAfterAntagSelected, after: [typeof(TraitorRuleSystem)]);
        SubscribeLocalEvent<TraitorUltraBountyTargetComponent, DamageChangedEvent>(OnBountyDamageChanged, before: [typeof(MobThresholdSystem)]);
        SubscribeLocalEvent<TraitorUltraBountyTargetComponent, MobStateChangedEvent>(OnBountyMobStateChanged);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _delayedActions.Clear();
    }

    private void OnAfterAntagSelected(Entity<TraitorUltraRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        EnsureInitialTraitorUltraSetup(ent, args.EntityUid, logMissingMind: true);
    }

    public void MakeAdminTraitorUltra(
        Entity<TraitorUltraRuleComponent, AntagSelectionComponent> rule,
        ICommonSession target,
        AntagSelectionDefinition definition)
    {
        var alreadyTraitor = _mind.TryGetMind(target, out var mindId, out _) &&
                             _roles.MindHasRole<TraitorRoleComponent>(mindId);

        if (!alreadyTraitor)
            _antag.MakeAntag((rule.Owner, rule.Comp2), target, definition);

        if (target.AttachedEntity is { } attached)
            EnsureInitialTraitorUltraSetup((rule.Owner, rule.Comp1), attached, logMissingMind: true);
    }

    private bool EnsureInitialTraitorUltraSetup(
        Entity<TraitorUltraRuleComponent> ent,
        EntityUid target,
        bool logMissingMind = false)
    {
        if (!_mind.TryGetMind(target, out var mindId, out var mind))
        {
            if (logMissingMind)
                Log.Error($"TraitorUltra selected {ToPrettyString(target)} but no mind was attached.");

            return false;
        }

        if (!ent.Comp.Minds.TryGetValue(mindId, out var state))
        {
            state = new TraitorUltraMindState
            {
                OriginalCorporation = GetOriginalCorporation(ent.Owner, mindId),
            };
            ent.Comp.Minds[mindId] = state;
        }
        else
        {
            state.OriginalCorporation ??= GetOriginalCorporation(ent.Owner, mindId);
        }

        if (state.Stage != TraitorUltraStage.Initial)
            return true;

        if (!state.BaseObjectivesAssigned)
        {
            if (TryAssignBaseObjectives((mindId, mind), ent.Comp, state.OriginalCorporation))
                state.BaseObjectivesAssigned = true;
            else
                Log.Error($"Failed to assign a base TraitorUltra objective package to {ToPrettyString(mindId)}.");
        }

        if (!state.InitialObjectivePackageAssigned)
        {
            if (AssignInitialObjectives((mindId, mind), ent.Comp, state))
                state.InitialObjectivePackageAssigned = true;
            else
                Log.Error($"Failed to assign a medium TraitorUltra objective package to {ToPrettyString(mindId)}.");
        }

        return state.BaseObjectivesAssigned && state.InitialObjectivePackageAssigned;
    }

    protected override void ActiveTick(EntityUid uid, TraitorUltraRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        ProcessDelayedActions();

        if (component.NextCheck > Timing.CurTime)
            return;

        component.NextCheck = Timing.CurTime + component.CheckDelay;

        foreach (var (mindId, state) in component.Minds)
        {
            if (!TryComp<MindComponent>(mindId, out var mind))
                continue;

            switch (state.Stage)
            {
                case TraitorUltraStage.Initial:
                    if (!InitialObjectivesCompleted(mindId, mind, component))
                        break;

                    ShowPopup(mind, "traitor-ultra-objectives-complete-popup", PopupType.Large);
                    state.Stage = TraitorUltraStage.CompletionPopupSent;
                    state.NextEventTime = Timing.CurTime + component.UpgradeOfferDelay;
                    break;

                case TraitorUltraStage.CompletionPopupSent:
                    if (Timing.CurTime < state.NextEventTime)
                        break;

                    if (TryGetSession(mind, out var session))
                    {
                        state.Stage = TraitorUltraStage.OfferOpen;
                        _eui.OpenEui(new TraitorUltraOfferEui(uid, mindId, this), session);
                    }
                    break;

                case TraitorUltraStage.Upgraded:
                    if (Timing.CurTime >= state.NextEventTime)
                        AnnounceBounty(uid, component, mindId, mind, state);
                    break;

                case TraitorUltraStage.BountyAnnounced:
                    EnsureBountyBody(uid, mindId, mind, state, replaceExisting: true);
                    break;
            }
        }
    }

    public void HandleUpgradeOffer(EntityUid rule, EntityUid mindId, bool accepted)
    {
        if (!TryComp<TraitorUltraRuleComponent>(rule, out var component) ||
            !component.Minds.TryGetValue(mindId, out var state) ||
            !TryComp<MindComponent>(mindId, out var mind) ||
            state.Stage != TraitorUltraStage.OfferOpen)
        {
            return;
        }

        if (!accepted)
        {
            state.Stage = TraitorUltraStage.Declined;
            ShowPopup(mind, "traitor-ultra-offer-declined-popup", PopupType.Large);
            return;
        }

        UpgradeTraitor(rule, component, mindId, mind, state);
    }

    public TraitorUltraOfferEuiState GetOfferState(EntityUid rule, EntityUid mindId)
    {
        string? oldCorp = null;
        string? newCorp = null;

        if (TryComp<TraitorUltraRuleComponent>(rule, out var component) &&
            component.Minds.TryGetValue(mindId, out var state))
        {
            state.OriginalCorporation ??= GetOriginalCorporation(rule, mindId);
            state.NewCorporation = PickNewCorporation(component, state.OriginalCorporation, state.NewCorporation);
            oldCorp = state.OriginalCorporation;
            newCorp = state.NewCorporation;
        }

        return new TraitorUltraOfferEuiState(
            Loc.GetString("traitor-ultra-offer-title"),
            Loc.GetString(
                "traitor-ultra-offer-body",
                ("oldCorp", LocalizeCorporation(oldCorp)),
                ("newCorp", LocalizeCorporation(newCorp))),
            Loc.GetString("traitor-ultra-offer-gains"),
            Loc.GetString("traitor-ultra-offer-losses"),
            Loc.GetString("traitor-ultra-offer-accept"),
            Loc.GetString("traitor-ultra-offer-decline"));
    }

    public void HandleRecruitOffer(EntityUid rule, EntityUid mindId, bool accepted)
    {
        if (!TryComp<TraitorUltraRuleComponent>(rule, out var component) ||
            !TryComp<MindComponent>(mindId, out var mind))
        {
            return;
        }

        if (!component.PendingRecruitOffers.Remove(mindId, out var corporation))
            return;

        if (!accepted || _roles.MindIsAntagonist(mindId))
            return;

        _roles.MindAddRole(mindId, component.RecruitMindRole, mind, silent: true);

        TraitorRuleComponent? traitorRule = null;
        if (TryComp<TraitorRuleComponent>(rule, out var existingTraitorRule))
        {
            traitorRule = existingTraitorRule;
            traitorRule.TraitorMinds.Add(mindId);

            if (!string.IsNullOrWhiteSpace(corporation))
                traitorRule.ObjectiveIssuersByMind[mindId] = corporation;
        }

        var assignedObjectives = new List<EntityUid>();
        if (!TryAssignRecruitObjectives((mindId, mind), component, corporation, assignedObjectives))
        {
            foreach (var objective in assignedObjectives)
                RemoveObjective((mindId, mind), objective);

            _roles.MindRemoveRole<TraitorRoleComponent>((mindId, mind));
            traitorRule?.TraitorMinds.Remove(mindId);
            traitorRule?.ObjectiveIssuersByMind.Remove(mindId);
            SendMindMessage(mind, "traitor-ultra-recruit-failed-no-objective", Color.OrangeRed);
            return;
        }

        if (mind.OwnedEntity is { } owned)
            _uplink.AddUplink(owned, component.RecruitTelecrystals, giveDiscounts: true);

        if (TryGetSession(mind, out var session))
        {
            _antag.SendBriefing(
                session,
                Loc.GetString("traitor-ultra-recruit-briefing", ("corp", LocalizeCorporation(corporation))),
                Color.Yellow,
                null);
        }
    }

    private bool TryAssignRecruitObjectives(
        Entity<MindComponent> mind,
        TraitorUltraRuleComponent component,
        string? issuer,
        List<EntityUid> assignedObjectives)
    {
        var excludedStealObjectives = _traitorRule.GetAssignedStealObjectivePrototypes(mind.Owner);
        var difficulty = 0f;
        for (var pick = 0;
             pick < component.RecruitObjectiveMaxPicks && component.RecruitObjectiveMaxDifficulty > difficulty;
             pick++)
        {
            var remainingDifficulty = component.RecruitObjectiveMaxDifficulty - difficulty;
            if (_objectives.GetRandomObjective(mind.Owner, mind.Comp, component.RecruitObjectiveGroups, remainingDifficulty, excludedStealObjectives) is not { } objective)
                continue;

            if (!string.IsNullOrWhiteSpace(issuer))
                _sharedObjectives.SetIssuer(objective, issuer);

            _mind.AddObjective(mind.Owner, mind.Comp, objective);

            if (!string.IsNullOrWhiteSpace(issuer))
                _sharedObjectives.SetIssuer(objective, issuer);

            assignedObjectives.Add(objective);
            TrackAssignedStealObjective(excludedStealObjectives, objective);
            difficulty += Comp<ObjectiveComponent>(objective).Difficulty;
        }

        return assignedObjectives.Count > 0;
    }

    private bool TryAssignBaseObjectives(
        Entity<MindComponent> mind,
        TraitorUltraRuleComponent component,
        string? issuer)
    {
        var excludedStealObjectives = _traitorRule.GetAssignedStealObjectivePrototypes(mind.Owner);
        var assigned = false;
        var difficulty = 0f;
        for (var pick = 0;
             pick < component.BaseObjectiveMaxPicks && component.BaseObjectiveMaxDifficulty > difficulty;
             pick++)
        {
            var remainingDifficulty = component.BaseObjectiveMaxDifficulty - difficulty;
            if (_objectives.GetRandomObjective(mind.Owner, mind.Comp, component.BaseObjectiveGroups, remainingDifficulty, excludedStealObjectives) is not { } objective)
                continue;

            if (!string.IsNullOrWhiteSpace(issuer))
                _sharedObjectives.SetIssuer(objective, issuer);

            _mind.AddObjective(mind.Owner, mind.Comp, objective);

            if (!string.IsNullOrWhiteSpace(issuer))
                _sharedObjectives.SetIssuer(objective, issuer);

            TrackAssignedStealObjective(excludedStealObjectives, objective);
            difficulty += Comp<ObjectiveComponent>(objective).Difficulty;
            assigned = true;
        }

        return assigned;
    }

    private bool AssignInitialObjectives(Entity<MindComponent> mind, TraitorUltraRuleComponent component, TraitorUltraMindState state)
    {
        if (HasHighRiskStealObjective(mind, component))
        {
            return TryAssignCommandKill(mind, component, state) ||
                   TryAssignHighRiskStealPackage(mind, component, state);
        }

        var preferStealPackage = _random.Prob(0.5f);
        return preferStealPackage
            ? TryAssignHighRiskStealPackage(mind, component, state) || TryAssignCommandKill(mind, component, state)
            : TryAssignCommandKill(mind, component, state) || TryAssignHighRiskStealPackage(mind, component, state);
    }

    private bool HasHighRiskStealObjective(Entity<MindComponent> mind, TraitorUltraRuleComponent component)
    {
        foreach (var objective in mind.Comp.Objectives)
        {
            if (TerminatingOrDeleted(objective))
                continue;

            var prototype = MetaData(objective).EntityPrototype?.ID;
            if (prototype != null && component.HighRiskStealObjectives.Contains(prototype))
                return true;
        }

        return false;
    }

    private bool TryAssignHighRiskStealPackage(Entity<MindComponent> mind, TraitorUltraRuleComponent component, TraitorUltraMindState state)
    {
        var excludedStealObjectives = _traitorRule.GetAssignedStealObjectivePrototypes(mind.Owner);
        var available = component.HighRiskStealObjectives
            .Where(proto => !excludedStealObjectives.Contains(proto))
            .ToList();
        var assigned = new List<EntityUid>();

        while (available.Count > 0 && assigned.Count < 2)
        {
            var proto = _random.PickAndTake(available);
            if (!TryCreateAndAddObjective(mind, proto, state.OriginalCorporation, out var objective))
                continue;

            assigned.Add(objective);
            excludedStealObjectives.Add(proto);
        }

        if (assigned.Count == 2)
        {
            state.InitialObjectives.AddRange(assigned);
            return true;
        }

        foreach (var objective in assigned)
            RemoveObjective(mind, objective);

        return false;
    }

    private void TrackAssignedStealObjective(HashSet<string> excludedStealObjectives, EntityUid objective)
    {
        if (HasComp<Content.Server.Objectives.Components.StealConditionComponent>(objective) &&
            MetaData(objective).EntityPrototype?.ID is { } prototype)
        {
            excludedStealObjectives.Add(prototype);
        }
    }

    private bool TryAssignCommandKill(Entity<MindComponent> mind, TraitorUltraRuleComponent component, TraitorUltraMindState state)
    {
        if (!TryCreateAndAddObjective(mind, component.CommandKillObjective, state.OriginalCorporation, out var objective))
            return false;

        state.InitialObjectives.Add(objective);
        return true;
    }

    private bool TryPickObjective(Entity<MindComponent> mind, IReadOnlyList<EntProtoId> prototypes, string? issuer, out EntityUid objective)
    {
        var available = prototypes.ToList();
        while (available.Count > 0)
        {
            var proto = _random.PickAndTake(available);
            if (TryCreateAndAddObjective(mind, proto, issuer, out objective))
                return true;
        }

        objective = default;
        return false;
    }

    private bool TryCreateAndAddObjective(Entity<MindComponent> mind, EntProtoId prototype, string? issuer, out EntityUid objective)
    {
        objective = default;
        if (!_proto.HasIndex<EntityPrototype>(prototype))
        {
            Log.Warning($"TraitorUltra objective prototype {prototype} does not exist.");
            return false;
        }

        var created = _sharedObjectives.TryCreateObjective(mind.Owner, mind.Comp, prototype);
        if (created == null)
            return false;

        objective = created.Value;
        if (!string.IsNullOrWhiteSpace(issuer))
            _sharedObjectives.SetIssuer(objective, issuer);

        _mind.AddObjective(mind.Owner, mind.Comp, objective);

        if (!string.IsNullOrWhiteSpace(issuer))
            _sharedObjectives.SetIssuer(objective, issuer);

        return true;
    }

    private void RemoveObjective(Entity<MindComponent> mind, EntityUid objective)
    {
        var index = mind.Comp.Objectives.IndexOf(objective);
        if (index >= 0)
            _mind.TryRemoveObjective(mind.Owner, mind.Comp, index);
        else
            Del(objective);
    }

    private bool InitialObjectivesCompleted(EntityUid mindId, MindComponent mind, TraitorUltraRuleComponent component)
    {
        var hasRequiredObjective = false;

        foreach (var objective in mind.Objectives.ToArray())
        {
            if (TerminatingOrDeleted(objective))
                return false;

            if (ObjectiveIgnoredForUpgradeCompletion(objective, component))
            {
                RemoveObjective((mindId, mind), objective);
                continue;
            }

            hasRequiredObjective = true;
            if (!_objectives.IsCompleted(objective, (mindId, mind)))
                return false;
        }

        return hasRequiredObjective;
    }

    private bool ObjectiveIgnoredForUpgradeCompletion(EntityUid objective, TraitorUltraRuleComponent component)
    {
        var prototype = MetaData(objective).EntityPrototype?.ID;
        if (prototype == null)
            return false;

        foreach (var ignored in component.UpgradeCompletionIgnoredObjectives)
        {
            if (ignored == prototype)
                return true;
        }

        return false;
    }

    private void UpgradeTraitor(EntityUid rule, TraitorUltraRuleComponent component, EntityUid mindId, MindComponent mind, TraitorUltraMindState state)
    {
        state.OriginalCorporation ??= GetOriginalCorporation(rule, mindId);
        state.NewCorporation = PickNewCorporation(component, state.OriginalCorporation, state.NewCorporation);
        state.AgentName = GetMindCharacterName(mind);
        state.BountyReward = component.TraitorKillRewardTelecrystals;
        state.Stage = TraitorUltraStage.Upgraded;
        state.NextEventTime = Timing.CurTime + component.BountyPreparationTime;

        UpgradeTraitorRole(mindId, mind);
        if (TryComp<TraitorRuleComponent>(rule, out var traitorRule))
            traitorRule.TraitorMinds.Remove(mindId);

        if (!TryAssignPostUpgradeObjective((mindId, mind), component, state))
            Log.Error($"Failed to assign a post-upgrade TraitorUltra objective to {ToPrettyString(mindId)}.");

        if (!TryAssignPostUpgradeSurviveObjective((mindId, mind), component, state.NewCorporation))
            Log.Error($"Failed to assign a post-upgrade TraitorUltra survive objective to {ToPrettyString(mindId)}.");

        AddTelecrystals(mind, component.UpgradeTelecrystals, component);
        SendUpgradeBriefing(mind, component, state);
        AppendUpgradeBriefing(mindId, state);
        LogTraitorUltraUpgrade(mindId, mind, state);
        QueueDelayedAction(component.BountyPreparationTime, rule, mindId, TraitorUltraDelayedActionType.AnnounceBounty);
    }

    private void LogTraitorUltraUpgrade(EntityUid mindId, MindComponent mind, TraitorUltraMindState state)
    {
        var characterName = state.AgentName ?? GetMindCharacterName(mind) ?? Loc.GetString("generic-unknown-title");
        var playerName = TryGetSession(mind, out var session) ? session.Name : Loc.GetString("generic-unknown-title");
        var oldCorporation = LocalizeCorporation(state.OriginalCorporation);
        var newCorporation = LocalizeCorporation(state.NewCorporation);
        var message = $"Агент {characterName} ({playerName}) стал ультра-предателем: {oldCorporation} -> {newCorporation}.";

        _chatManager.SendAdminAlert(message);
        _adminLogger.Add(LogType.AntagSelection, LogImpact.High, $"{ToPrettyString(mindId)} became TraitorUltra. Player: {playerName}, character: {characterName}, corporation: {oldCorporation} -> {newCorporation}.");
    }

    private bool TryAssignPostUpgradeObjective(Entity<MindComponent> mind, TraitorUltraRuleComponent component, TraitorUltraMindState state)
    {
        if (_random.Prob(component.RarePostUpgradeObjectiveProbability))
        {
            if (TryPickObjective(mind, component.RarePostUpgradeObjectives, state.NewCorporation, out _))
                return true;
        }

        if (TryPickObjective(mind, component.PostUpgradeObjectives, state.NewCorporation, out _))
            return true;

        return TryPickObjective(mind, component.RarePostUpgradeObjectives, state.NewCorporation, out _);
    }

    private bool TryAssignPostUpgradeSurviveObjective(Entity<MindComponent> mind, TraitorUltraRuleComponent component, string? issuer)
    {
        return TryCreateAndAddObjective(mind, component.PostUpgradeSurviveObjective, issuer, out _);
    }

    private void UpgradeTraitorRole(EntityUid mindId, MindComponent mind)
    {
        if (!_roles.MindHasRole<TraitorRoleComponent>(mindId, out var traitorRole))
        {
            Log.Warning($"TraitorUltra upgrade could not find a TraitorRole on {ToPrettyString(mindId)}; adding Ultra role silently.");
            _roles.MindAddRole(mindId, "MindRoleTraitorUltra", mind, silent: true);
            return;
        }

        var roleUid = traitorRole.Value.Owner;
        var role = traitorRole.Value.Comp1;
        role.AntagPrototype = "TraitorUltra";
        role.Subtype = "role-subtype-traitor-ultra";
        EnsureComp<TraitorUltraRoleComponent>(roleUid);
        Dirty(roleUid, role);
        _roles.RefreshMindRoleType((mindId, mind));
    }

    private void AppendUpgradeBriefing(EntityUid mindId, TraitorUltraMindState state)
    {
        if (!_roles.MindHasRole<TraitorRoleComponent>(mindId, out var traitorRole))
            return;

        var roleUid = traitorRole.Value.Owner;
        var briefing = EnsureComp<RoleBriefingComponent>(roleUid);
        var ultraBriefing = Loc.GetString(
            "traitor-ultra-role-briefing-memory",
            ("oldCorp", LocalizeCorporation(state.OriginalCorporation)),
            ("newCorp", LocalizeCorporation(state.NewCorporation)));

        briefing.Briefing = string.IsNullOrWhiteSpace(briefing.Briefing)
            ? ultraBriefing
            : $"{briefing.Briefing}\n{ultraBriefing}";
        Dirty(roleUid, briefing);
    }

    private void SendUpgradeBriefing(MindComponent mind, TraitorUltraRuleComponent component, TraitorUltraMindState state)
    {
        if (!TryGetSession(mind, out var session))
            return;

        var briefing = Loc.GetString(
            "traitor-ultra-upgrade-briefing",
            ("oldCorp", LocalizeCorporation(state.OriginalCorporation)),
            ("newCorp", LocalizeCorporation(state.NewCorporation)));

        _antag.SendBriefing(session, briefing, Color.Yellow, component.UpgradeSound);
    }

    private void AnnounceBounty(EntityUid rule, TraitorUltraRuleComponent component, EntityUid mindId, MindComponent? mind, TraitorUltraMindState state)
    {
        if (state.BountyAnnounced)
            return;

        state.BountyAnnounced = true;
        state.Stage = TraitorUltraStage.BountyAnnounced;

        var name = state.AgentName ?? (mind == null ? null : GetMindCharacterName(mind));
        state.AgentName = name;

        var announcement = Loc.GetString(
            GetBountyAnnouncementLocId(state.OriginalCorporation),
            ("oldCorp", LocalizeCorporation(state.OriginalCorporation)),
            ("newCorp", LocalizeCorporation(state.NewCorporation)),
            ("agent", name ?? Loc.GetString("generic-unknown-title")),
            ("reward", state.BountyReward));

        _chat.DispatchGlobalAnnouncement(
            announcement,
            sender: LocalizeCorporation(state.OriginalCorporation),
            playSound: true,
            announcementSound: component.BountyAnnouncementSound,
            colorOverride: Color.OrangeRed,
            originalMessage: announcement,
            voice: PickRandomAnnouncementVoice());

        if (mind != null)
            EnsureBountyBody(rule, mindId, mind, state, replaceExisting: true);
    }

    private void EnsureBountyBody(EntityUid rule, EntityUid mindId, MindComponent mind, TraitorUltraMindState state, bool replaceExisting)
    {
        if (state.BountyResolved)
            return;

        if (!replaceExisting && state.BountyBody is { } existingBody && !TerminatingOrDeleted(existingBody))
        {
            var existingComp = EnsureComp<TraitorUltraBountyTargetComponent>(existingBody);
            existingComp.Rule = rule;
            existingComp.MindId = mindId;
            return;
        }

        if (mind.OwnedEntity is not { } body || TerminatingOrDeleted(body))
            return;

        if (replaceExisting &&
            state.BountyBody is { } oldBody &&
            oldBody != body &&
            !TerminatingOrDeleted(oldBody))
        {
            RemCompDeferred<TraitorUltraBountyTargetComponent>(oldBody);
        }

        state.BountyBody = body;
        var comp = EnsureComp<TraitorUltraBountyTargetComponent>(body);
        comp.Rule = rule;
        comp.MindId = mindId;
    }

    private void OnBountyDamageChanged(Entity<TraitorUltraBountyTargetComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
        {
            if (args.Damageable.TotalDamage == FixedPoint2.Zero)
            {
                ent.Comp.DamageByMind.Clear();
                ent.Comp.DamageSourceByMind.Clear();
            }

            return;
        }

        var delta = args.DamageDelta.GetTotal();
        if (!args.DamageIncreased)
        {
            if (args.Damageable.TotalDamage == FixedPoint2.Zero)
            {
                ent.Comp.DamageByMind.Clear();
                ent.Comp.DamageSourceByMind.Clear();
            }

            return;
        }

        if (delta <= FixedPoint2.Zero ||
            !TryGetDamageSourceMind(args.Origin, out var sourceMindId, out _, out var sourceEntity) ||
            sourceMindId == ent.Comp.MindId && sourceEntity == ent.Owner)
        {
            return;
        }

        ent.Comp.DamageByMind[sourceMindId] = ent.Comp.DamageByMind.GetValueOrDefault(sourceMindId) + delta;
        ent.Comp.DamageSourceByMind[sourceMindId] = sourceEntity;
    }

    private void OnBountyMobStateChanged(Entity<TraitorUltraBountyTargetComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || args.OldMobState >= args.NewMobState)
            return;

        if (!TryComp<TraitorUltraRuleComponent>(ent.Comp.Rule, out var ruleComp) ||
            !ruleComp.Minds.TryGetValue(ent.Comp.MindId, out var state) ||
            state.BountyResolved)
        {
            return;
        }

        state.BountyResolved = true;
        state.Stage = TraitorUltraStage.Resolved;
        RemCompDeferred<TraitorUltraBountyTargetComponent>(ent);

        if (!TryResolveKillerMind(ent.Comp, args.Origin, out var killerMindId, out var killerMind, out var killerEntity) ||
            killerMindId == ent.Comp.MindId && (killerEntity == null || killerEntity == ent.Owner))
        {
            return;
        }

        if (_roles.MindHasRole<TraitorRoleComponent>(killerMindId))
        {
            SendMindMessage(killerMind, "traitor-ultra-bounty-traitor-kill-message", Color.Yellow);
            QueueDelayedAction(ruleComp.RewardDelay, ent.Comp.Rule, killerMindId, TraitorUltraDelayedActionType.GiveTraitorKillReward);
            return;
        }

        if (IsCaptainMind(killerMindId))
        {
            if (!TryCreditCaptainKillReward(killerMind, killerEntity, ruleComp))
                Log.Warning($"Failed to credit TraitorUltra captain kill reward to {ToPrettyString(killerMindId)}.");

            SendMindMessage(killerMind, "traitor-ultra-bounty-captain-kill-message", Color.LightSkyBlue);
            return;
        }

        if (HasRealMindShield(killerMind, killerEntity))
        {
            if (!TryCreditSecurityKillReward(ent.Owner, killerMind, killerEntity, ruleComp))
                Log.Warning($"Failed to credit TraitorUltra security kill reward for {ToPrettyString(killerMindId)}.");

            SendMindMessage(killerMind, "traitor-ultra-bounty-security-kill-message", Color.LightSkyBlue);
            return;
        }

        if (!_roles.MindIsAntagonist(killerMindId) && IsPlayerMind(killerMind))
        {
            SendMindMessage(killerMind, "traitor-ultra-bounty-crew-kill-message", Color.Yellow);
            QueueDelayedAction(ruleComp.RewardDelay, ent.Comp.Rule, killerMindId, TraitorUltraDelayedActionType.OpenRecruitOffer, state.OriginalCorporation);
        }
    }

    private bool TryResolveKillerMind(
        TraitorUltraBountyTargetComponent comp,
        EntityUid? origin,
        out EntityUid mindId,
        out MindComponent mind,
        out EntityUid? sourceEntity)
    {
        if (TryGetDamageSourceMind(origin, out mindId, out mind, out var directSource))
        {
            sourceEntity = directSource;
            return true;
        }

        var highest = FixedPoint2.Zero;
        mindId = default;
        mind = default!;
        sourceEntity = null;
        var found = false;

        foreach (var (candidateMindId, damage) in comp.DamageByMind)
        {
            if (damage <= highest || !TryComp<MindComponent>(candidateMindId, out var candidateMind))
                continue;

            mindId = candidateMindId;
            mind = candidateMind;
            sourceEntity = comp.DamageSourceByMind.GetValueOrDefault(candidateMindId);
            highest = damage;
            found = true;
        }

        return found;
    }

    private bool TryGetDamageSourceMind(EntityUid? source, out EntityUid mindId, out MindComponent mind, out EntityUid sourceEntity)
    {
        mindId = default;
        mind = default!;
        sourceEntity = default;

        if (source == null)
            return false;

        if (TryGetMind(source.Value, out mindId, out mind))
        {
            sourceEntity = source.Value;
            return true;
        }

        if (TryGetProjectileSourceMind(source.Value, out mindId, out mind, out sourceEntity))
            return true;

        var current = source.Value;
        for (var i = 0; i < SourceParentSearchDepth; i++)
        {
            if (!TryComp(current, out TransformComponent? transform))
                return false;

            var parent = transform.ParentUid;
            if (parent == current)
                return false;

            if (TryGetMind(parent, out mindId, out mind))
            {
                sourceEntity = parent;
                return true;
            }

            if (TryGetProjectileSourceMind(parent, out mindId, out mind, out sourceEntity))
                return true;

            current = parent;
        }

        return false;
    }

    private bool TryGetProjectileSourceMind(EntityUid uid, out EntityUid mindId, out MindComponent mind, out EntityUid sourceEntity)
    {
        mindId = default;
        mind = default!;
        sourceEntity = default;

        if (!TryComp<ProjectileComponent>(uid, out var projectile))
            return false;

        if (projectile.Shooter != null && TryGetMind(projectile.Shooter.Value, out mindId, out mind))
        {
            sourceEntity = projectile.Shooter.Value;
            return true;
        }

        if (projectile.Weapon == null || !TryGetMind(projectile.Weapon.Value, out mindId, out mind))
            return false;

        sourceEntity = projectile.Weapon.Value;
        return true;
    }

    private bool TryGetMind(EntityUid uid, out EntityUid mindId, out MindComponent mind)
    {
        mindId = default;
        mind = default!;

        if (!TryComp<MindContainerComponent>(uid, out var mindContainer) || mindContainer.Mind == null)
            return false;

        mindId = mindContainer.Mind.Value;
        if (!TryComp<MindComponent>(mindId, out var mindComp))
            return false;

        mind = mindComp;
        return true;
    }

    private bool IsCaptainMind(EntityUid mindId)
    {
        return _jobs.MindTryGetJobId(mindId, out var jobId) &&
               jobId != null &&
               jobId.Value == "Captain";
    }

    private bool HasRealMindShield(MindComponent killerMind, EntityUid? entity)
    {
        var killerEntity = GetKillerRewardEntity(killerMind, entity);
        return killerEntity != null && HasComp<MindShieldComponent>(killerEntity.Value);
    }

    private bool TryCreditCaptainKillReward(
        MindComponent killerMind,
        EntityUid? sourceEntity,
        TraitorUltraRuleComponent component)
    {
        if (component.CaptainKillRewardCredits <= 0)
            return true;

        var killerEntity = GetKillerRewardEntity(killerMind, sourceEntity);
        if (killerEntity == null || !_idCard.TryFindIdCard(killerEntity.Value, out var idCard))
            return false;

        if (!_bankManager.TryGetBankAccount(idCard.Owner, out var account))
        {
            account = _bankManager.CreateNewBankAccount(idCard.Owner);
            if (account == null)
                return false;

            account.Value.Comp.AccountName = GetRewardAccountName(killerEntity.Value, idCard);
            idCard.Comp.StoredBankAccountNumber = account.Value.Comp.AccountNumber;
            Dirty(idCard);
            Dirty(account.Value);
        }
        else if (string.IsNullOrWhiteSpace(idCard.Comp.StoredBankAccountNumber))
        {
            idCard.Comp.StoredBankAccountNumber = account.Value.Comp.AccountNumber;
            Dirty(idCard);
        }

        return _bankManager.TryChangeBalanceBy(account.Value, FixedPoint2.New(component.CaptainKillRewardCredits));
    }

    private bool TryCreditSecurityKillReward(
        EntityUid bountyBody,
        MindComponent killerMind,
        EntityUid? sourceEntity,
        TraitorUltraRuleComponent component)
    {
        if (component.SecurityKillRewardCredits <= 0)
            return true;

        var station = _station.GetOwningStation(bountyBody) ??
                      _station.GetOwningStation(sourceEntity) ??
                      _station.GetOwningStation(killerMind.OwnedEntity);

        if (station == null || !TryComp<StationBankAccountComponent>(station.Value, out var bank))
            return false;

        _cargo.UpdateBankAccount((station.Value, bank), component.SecurityKillRewardCredits, component.SecurityRewardAccount);
        return true;
    }

    private EntityUid? GetKillerRewardEntity(MindComponent killerMind, EntityUid? sourceEntity)
    {
        return sourceEntity != null && !TerminatingOrDeleted(sourceEntity.Value)
            ? sourceEntity.Value
            : killerMind.OwnedEntity;
    }

    private string GetRewardAccountName(EntityUid killerEntity, Entity<IdCardComponent> idCard)
    {
        return string.IsNullOrWhiteSpace(idCard.Comp.FullName)
            ? Name(killerEntity)
            : idCard.Comp.FullName;
    }

    private void AddTelecrystals(MindComponent mind, FixedPoint2 amount, TraitorUltraRuleComponent component)
    {
        if (amount <= FixedPoint2.Zero || mind.OwnedEntity is not { } owned)
            return;

        var uplink = _uplink.FindUplinkTarget(owned);
        if (uplink == null || !TryComp<StoreComponent>(uplink.Value, out var store))
        {
            _uplink.AddUplink(owned, amount, giveDiscounts: true);
            return;
        }

        _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { component.TelecrystalCurrency, amount } }, uplink.Value, store);
        _store.UpdateUserInterface(owned, uplink.Value, store);
    }

    private void QueueDelayedAction(TimeSpan delay, EntityUid rule, EntityUid mindId, TraitorUltraDelayedActionType type, string? corporation = null)
    {
        _delayedActions.Add(new TraitorUltraDelayedAction(Timing.CurTime + delay, rule, mindId, type, corporation));
    }

    private void ProcessDelayedActions()
    {
        for (var i = _delayedActions.Count - 1; i >= 0; i--)
        {
            var action = _delayedActions[i];
            if (Timing.CurTime < action.At)
                continue;

            _delayedActions.RemoveAt(i);

            if (!TryComp<TraitorUltraRuleComponent>(action.Rule, out var component))
            {
                continue;
            }

            switch (action.Type)
            {
                case TraitorUltraDelayedActionType.AnnounceBounty:
                    if (component.Minds.TryGetValue(action.MindId, out var state))
                    {
                        TryComp<MindComponent>(action.MindId, out var announceMind);
                        AnnounceBounty(action.Rule, component, action.MindId, announceMind, state);
                    }
                    break;

                case TraitorUltraDelayedActionType.GiveTraitorKillReward:
                    if (TryComp<MindComponent>(action.MindId, out var rewardMind))
                        AddTelecrystals(rewardMind, component.TraitorKillRewardTelecrystals, component);
                    break;

                case TraitorUltraDelayedActionType.OpenRecruitOffer:
                    if (TryComp<MindComponent>(action.MindId, out var recruitMind) &&
                        !_roles.MindIsAntagonist(action.MindId) &&
                        TryGetSession(recruitMind, out var session))
                    {
                        component.PendingRecruitOffers[action.MindId] = action.Corporation;
                        _eui.OpenEui(new TraitorUltraRecruitEui(action.Rule, action.MindId, this, action.Corporation), session);
                    }
                    break;
            }
        }
    }

    private string? GetOriginalCorporation(EntityUid rule, EntityUid mindId)
    {
        if (!TryComp<TraitorRuleComponent>(rule, out var traitorRule))
            return null;

        return traitorRule.ObjectiveIssuersByMind.GetValueOrDefault(mindId);
    }

    private string? PickNewCorporation(TraitorUltraRuleComponent component, string? original, string? current = null)
    {
        if (!_proto.TryIndex(component.CorporationDataset, out var dataset) || dataset.Values.Count == 0)
            return current;

        if (!string.IsNullOrWhiteSpace(current) && !CorporationMatches(current, original))
            return NormalizeCorporation(dataset, current);

        var values = dataset.Values.ToList();
        if (original != null && values.Count > 1)
            values.RemoveAll(value => CorporationMatches(value, original));

        return values.Count == 0
            ? NormalizeCorporation(dataset, current)
            : _random.Pick(values);
    }

    private string? NormalizeCorporation(LocalizedDatasetPrototype dataset, string? corporation)
    {
        if (string.IsNullOrWhiteSpace(corporation))
            return null;

        foreach (var locId in dataset.Values)
        {
            if (CorporationMatches(locId, corporation))
                return locId;
        }

        return corporation;
    }

    private bool CorporationMatches(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return false;

        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
            return true;

        return string.Equals(LocalizeCorporation(left), LocalizeCorporation(right), StringComparison.OrdinalIgnoreCase);
    }

    private string LocalizeCorporation(string? locId)
    {
        return string.IsNullOrWhiteSpace(locId)
            ? Loc.GetString("objective-issuer-unknown")
            : Loc.GetString(locId);
    }

    private string GetBountyAnnouncementLocId(string? locId)
    {
        return locId switch
        {
            "traitor-corporations-dataset-1" => "traitor-ultra-bounty-announcement-cybersun",
            "traitor-corporations-dataset-2" => "traitor-ultra-bounty-announcement-gorlex",
            "traitor-corporations-dataset-3" => "traitor-ultra-bounty-announcement-interdyne",
            "traitor-corporations-dataset-7" => "traitor-ultra-bounty-announcement-donk",
            _ => "traitor-ultra-bounty-announcement",
        };
    }

    private string? PickRandomAnnouncementVoice()
    {
        var voices = _proto.EnumeratePrototypes<TTSVoicePrototype>().ToArray();
        return voices.Length == 0 ? null : _random.Pick(voices).ID;
    }

    private string? GetMindCharacterName(MindComponent mind)
    {
        var name = mind.CharacterName;
        if (string.IsNullOrWhiteSpace(name) && mind.OwnedEntity is { } owned)
            name = Name(owned);

        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private bool TryGetSession(MindComponent mind, out ICommonSession session)
    {
        session = default!;
        if (mind.UserId == null || !_players.TryGetSessionById(mind.UserId.Value, out var found))
            return false;

        session = found;
        return true;
    }

    private void ShowPopup(MindComponent mind, string locId, PopupType type)
    {
        if (mind.OwnedEntity is { } owned)
            _popup.PopupEntity(Loc.GetString(locId), owned, owned, type);
    }

    private void SendMindMessage(MindComponent mind, string locId, Color color)
    {
        if (!TryGetSession(mind, out var session))
            return;

        var message = Loc.GetString(locId);
        var wrapped = Loc.GetString("chat-manager-server-wrap-message", ("message", FormattedMessage.EscapeText(message)));
        ChatManager.ChatMessageToOne(ChatChannel.Server, message, wrapped, default, false, session.Channel, color);
    }

    private static bool IsPlayerMind(MindComponent mind)
    {
        return mind.UserId != null || mind.OriginalOwnerUserId != null;
    }
}

public readonly record struct TraitorUltraDelayedAction(
    TimeSpan At,
    EntityUid Rule,
    EntityUid MindId,
    TraitorUltraDelayedActionType Type,
    string? Corporation = null);

public enum TraitorUltraDelayedActionType : byte
{
    AnnounceBounty,
    GiveTraitorKillReward,
    OpenRecruitOffer,
}
