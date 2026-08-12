// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Shared.DeadSpace.Clothing.ReverseRig;

public sealed class ReverseRigSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReverseRigComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ReverseRigComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ReverseRigComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<ReverseRigComponent, GotUnequippedEvent>(OnGotUnequipped);
        SubscribeLocalEvent<ReverseRigComponent, ComponentRemove>(OnComponentRemove);

        SubscribeLocalEvent<ReverseRigBackpackComponent, BeingUnequippedAttemptEvent>(OnBackpackUnequipAttempt);
        SubscribeLocalEvent<ReverseRigBackpackComponent, ComponentRemove>(OnBackpackComponentRemove);
    }

    private void OnComponentInit(Entity<ReverseRigComponent> ent, ref ComponentInit args)
    {
        var (uid, component) = ent;
        component.BackpackContainer = _container.EnsureContainer<ContainerSlot>(uid, component.BackpackContainerId);
    }

    private void OnMapInit(Entity<ReverseRigComponent> ent, ref MapInitEvent args)
    {
        var (uid, component) = ent;
        var container = component.BackpackContainer;
        if (container == null)
            return;

        // A backpack already exists (e.g. the suit was mapped in with one).
        if (container.ContainedEntity is { } existing)
        {
            if (component.BackpackUid != existing)
            {
                component.BackpackUid = existing;
                Dirty(uid, component);
            }
            return;
        }

        var xform = Transform(uid);
        var backpack = Spawn(component.BackpackPrototype, xform.Coordinates);
        component.BackpackUid = backpack;
        var attached = EnsureComp<ReverseRigBackpackComponent>(backpack);
        attached.AttachedUid = uid;
        Dirty(backpack, attached);

        _container.Insert(backpack, container, containerXform: xform);
        Dirty(uid, component);
    }

    private void OnGotEquipped(Entity<ReverseRigComponent> ent, ref GotEquippedEvent args)
    {
        var (uid, component) = ent;
        if ((args.SlotFlags & component.RequiredFlags) != component.RequiredFlags)
            return;

        if (component.BackpackUid is not { } backpack || Deleted(backpack))
            return;

        var wearer = args.Equipee;

        // Whatever previously occupied the slot falls off the wearer, unless it is our own backpack.
        if (_inventory.TryGetSlotEntity(wearer, component.Slot, out var existing) && existing != backpack)
        {
            _inventory.TryUnequip(wearer, component.Slot, force: true, triggerHandContact: true);
        }

        _inventory.TryEquip(wearer, wearer, backpack, component.Slot, force: true, triggerHandContact: true);
    }

    private void OnGotUnequipped(Entity<ReverseRigComponent> ent, ref GotUnequippedEvent args)
    {
        var (uid, component) = ent;
        if ((args.SlotFlags & component.RequiredFlags) != component.RequiredFlags)
            return;

        if (component.BackpackUid is not { } backpack || Deleted(backpack))
            return;

        var wearer = args.Equipee;

        // The backpack comes off together with the suit.
        if (_inventory.TryGetSlotEntity(wearer, component.Slot, out var existing) && existing == backpack)
            _inventory.TryUnequip(wearer, component.Slot, force: true, triggerHandContact: true);

        if (component.BackpackContainer != null)
            _container.Insert(backpack, component.BackpackContainer);
    }

    private void OnComponentRemove(Entity<ReverseRigComponent> ent, ref ComponentRemove args)
    {
        var (_, component) = ent;
        if (component.BackpackUid is { } backpack && !Deleted(backpack))
            QueueDel(backpack);
    }

    private void OnBackpackUnequipAttempt(Entity<ReverseRigBackpackComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        // The backpack is permanently attached to the suit and can not be removed manually.
        args.Cancel();
    }

    private void OnBackpackComponentRemove(Entity<ReverseRigBackpackComponent> ent, ref ComponentRemove args)
    {
        // The backpack was removed or destroyed - clear the suit's reference.
        if (ent.Comp.AttachedUid is { } suit && TryComp<ReverseRigComponent>(suit, out var rig))
        {
            rig.BackpackUid = null;
            Dirty(suit, rig);
        }
    }
}
