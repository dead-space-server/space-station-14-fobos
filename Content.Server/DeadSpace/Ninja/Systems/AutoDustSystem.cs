using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.DeadSpace.Ninja.Systems;
using Content.Shared.Gibbing;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Popups;

namespace Content.Server.DeadSpace.Ninja.Systems;

public sealed class AutoDustSystem : SharedAutoDustSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoDustMarkerComponent, MobStateChangedEvent>(OnMobState);

        SubscribeLocalEvent<AutoDustComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<AutoDustComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<AutoDustComponent, ToggleAutoDustModeActionEvent>(OnToggleMode);
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
                _popup.PopupEntity(Loc.GetString("auto-dust-toggle-crit"), args.Performer, PopupType.MediumCaution);
                break;
            case DustMode.Crit:
                comp.AutoDustMode = DustMode.Dead;
                _popup.PopupEntity(Loc.GetString("auto-dust-toggle-dead"), args.Performer, PopupType.MediumCaution);
                break;
            case DustMode.Dead:
                comp.AutoDustMode = DustMode.Off;
                _popup.PopupEntity(Loc.GetString("auto-dust-toggle-off"), args.Performer, PopupType.MediumCaution);
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
            ActivateAutoDust(ent.Owner);
        }

        if (args.NewMobState == MobState.Critical && dust.AutoDustMode == DustMode.Crit)
        {
            ActivateAutoDust(ent.Owner);
        }
    }

    public void ActivateAutoDust(Entity<AutoDustMarkerComponent> ent)
    {
        var mapCoords = _transform.GetMapCoordinates(ent.Owner);
        TryComp<AutoDustComponent>(ent.Comp.AutoDustItem, out var dust);
        Spawn(component.SpawnOnDustProto, mapCoords);

        if (component.DeleteItems)
        {
            var items = _inventory.GetHandOrInventoryEntities(ent.Owner);
            foreach (var item in items)
            {
                QueueDel(ent.Owner);
            }
        }

        _gibbing.Gib(uid);
    }
}