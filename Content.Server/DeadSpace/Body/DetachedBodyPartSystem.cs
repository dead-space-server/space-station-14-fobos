// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.DeadSpace.Necromorphs.PlasmaCutter;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Standing;

namespace Content.Server.DeadSpace.Body;

/// <summary>
/// Keeps equipment and identity consistent when visible body parts are severed.
/// </summary>
public sealed class DetachedBodyPartSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BodyComponent, BodyPartRemovedEvent>(OnBodyPartRemoved);
        SubscribeLocalEvent<BodyComponent, StandAttemptEvent>(OnStandAttempt);
        SubscribeLocalEvent<NecromorphMissingHeadComponent, IsEquippingTargetAttemptEvent>(OnMissingHeadEquipAttempt);
        SubscribeLocalEvent<NecromorphPlasmaCutterWoundsComponent, IsEquippingTargetAttemptEvent>(OnMissingLegEquipAttempt);
    }

    private void OnStandAttempt(Entity<BodyComponent> body, ref StandAttemptEvent args)
    {
        if (body.Comp.RequiredLegs <= 0)
            return;

        if (body.Comp.LegEntities.Count < body.Comp.RequiredLegs)
            args.Cancel();
    }

    private void OnMissingHeadEquipAttempt(
        Entity<NecromorphMissingHeadComponent> ent,
        ref IsEquippingTargetAttemptEvent args)
    {
        CancelEquip(args, SlotFlags.HEAD | SlotFlags.MASK | SlotFlags.EYES | SlotFlags.EARS);
    }

    private void OnMissingLegEquipAttempt(
        Entity<NecromorphPlasmaCutterWoundsComponent> ent,
        ref IsEquippingTargetAttemptEvent args)
    {
        if (ent.Comp.RemovedLegs > 0)
            CancelEquip(args, SlotFlags.LEGS | SlotFlags.FEET | SlotFlags.SOCKS);
    }

    private static void CancelEquip(IsEquippingTargetAttemptEvent args, SlotFlags blocked)
    {
        if ((args.SlotFlags & blocked) == 0)
            return;

        args.Reason = "inventory-component-can-equip-cannot";
        args.Cancel();
    }

    private void OnBodyPartRemoved(Entity<BodyComponent> ent, ref BodyPartRemovedEvent args)
    {
        switch (args.Part.Comp.PartType)
        {
            case BodyPartType.Leg:
                DropEquipment(ent.Owner, SlotFlags.LEGS | SlotFlags.FEET | SlotFlags.SOCKS);
                // The removed leg is still present in LegEntities while this event is raised.
                if (ent.Comp.RequiredLegs > 0
                    && ent.Comp.LegEntities.Count - 1 < ent.Comp.RequiredLegs)
                    _standing.Down(ent.Owner);
                break;
            case BodyPartType.Head:
                DropEquipment(ent.Owner, SlotFlags.HEAD | SlotFlags.MASK | SlotFlags.EYES | SlotFlags.EARS);
                _metaData.SetEntityName(args.Part.Owner,
                    Loc.GetString("detached-head-name", ("name", Name(ent.Owner))));
                break;
        }
    }

    private void DropEquipment(EntityUid body, SlotFlags flags)
    {
        var slots = _inventory.GetSlotEnumerator(body, flags);
        while (slots.NextItem(out _, out var slot))
            _inventory.TryUnequip(body, body, slot.Name, force: true, triggerHandContact: true);
    }
}
