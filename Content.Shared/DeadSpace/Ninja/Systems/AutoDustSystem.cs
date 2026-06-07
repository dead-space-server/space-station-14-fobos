using Content.Shared.Actions;
using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Gibbing;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Popups;

namespace Content.Shared.DeadSpace.Ninja.Systems;

public sealed class AutoDustSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoDustComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AutoDustComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<AutoDustComponent, ToggleAutoDustModeActionEvent>(OnToggleMode);
        SubscribeLocalEvent<AutoDustMarkerComponent, MobStateChangedEvent>(OnMobState);

        SubscribeLocalEvent<AutoDustComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<AutoDustComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnMapInit(Entity<AutoDustComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;
        _actions.AddAction(uid, ref comp.ActionEntity, comp.Action);
        Dirty(uid, comp);
    }

    private void OnGetActions(Entity<AutoDustComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;
        args.AddAction(ent.Comp.ActionEntity);
    }

    private void OnEquipped(Entity<AutoDustComponent> ent, ref GotEquippedEvent args)
    {
        EnsureComp<AutoDustMarkerComponent>(args.Equipee).AutoDustItem = ent.Owner;
    }

    private void OnUnequipped(Entity<AutoDustComponent> ent, ref GotUnequippedEvent args)
    {
        RemComp<AutoDustMarkerComponent>(args.Equipee);
    }

    private void OnToggleMode(Entity<AutoDustComponent> ent, ref ToggleAutoDustModeActionEvent args)
    {
        args.Handled = true;
        var (uid, comp) = ent;
        switch (comp.AutoDustMode)
        {
            case DustMode.Off:
                comp.AutoDustMode = DustMode.Crit;
                _popup.PopupClient(Loc.GetString("auto-dust-toggle-crit"), args.Performer, args.Performer, PopupType.LargeCaution);
                break;
            case DustMode.Crit:
                comp.AutoDustMode = DustMode.Dead;
                _popup.PopupClient(Loc.GetString("auto-dust-toggle-dead"), args.Performer, args.Performer, PopupType.LargeCaution);
                break;
            case DustMode.Dead:
                comp.AutoDustMode = DustMode.Off;
                _popup.PopupClient(Loc.GetString("auto-dust-toggle-off"), args.Performer, args.Performer, PopupType.LargeCaution);
                break;
        }
        Dirty(uid, comp);
    }

    private void OnMobState(Entity<AutoDustMarkerComponent> ent, ref MobStateChangedEvent args)
    {
        if (!TryComp<AutoDustComponent>(ent.Comp.AutoDustItem, out var dust))
            return;

        if (args.NewMobState == MobState.Dead && dust.AutoDustMode == DustMode.Dead)
        {
            ActivateAutoDust(ent.Owner, dust);
        }

        if (args.NewMobState == MobState.Critical && dust.AutoDustMode == DustMode.Crit)
        {
            ActivateAutoDust(ent.Owner, dust);
        }
    }

    public void ActivateAutoDust(EntityUid uid, AutoDustComponent component)
    {
        var mapCoords = _transform.GetMapCoordinates(uid);
        Spawn(component.SpawnOnDustProto, mapCoords);

        if (component.DeleteItems)
        {
            var items = _inventory.GetHandOrInventoryEntities(uid);
            foreach (var item in items)
            {
                QueueDel(item);
            }
        }

        _gibbing.Gib(uid);
    }
}