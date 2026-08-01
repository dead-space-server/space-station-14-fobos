using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Content.Shared.Interaction.Components;
using Robust.Shared.Network;
using System.Linq;

namespace Content.Shared.DeadSpace.Clothing;

public sealed class MultiClothingSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly INetManager _net = default!;

    public const string ContainerId = "multi-clothing-container";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultiClothingComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<IsEquippingAttemptEvent>(OnEquippingAttempt);
        SubscribeLocalEvent<MultiClothingComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<MultiClothingComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnMapInit(Entity<MultiClothingComponent> ent, ref MapInitEvent args)
    {
        _container.EnsureContainer<Container>(ent, ContainerId);
    }

    private void OnEquippingAttempt(IsEquippingAttemptEvent args)
    {
        if (!TryComp<MultiClothingComponent>(args.Equipment, out var component))
            return;

        foreach (var (slotName, _) in component.Equipment)
        {
            if (!_inventory.TryGetSlot(args.EquipTarget, slotName, out _))
            {
                args.Cancel();
                return;
            }

            if (!component.Force && _inventory.TryGetSlotEntity(args.EquipTarget, slotName, out _))
            {
                args.Cancel();
                return;
            }
        }
    }

    private void OnGotEquipped(Entity<MultiClothingComponent> ent, ref GotEquippedEvent args)
    {
        if (!_net.IsServer)
            return;
        var container = _container.EnsureContainer<Container>(ent, ContainerId);
        var equipped = new Dictionary<string, EntityUid>();
        var forcedOff = new Dictionary<string, EntityUid>();

        foreach (var (slotName, proto) in ent.Comp.Equipment)
        {
            if (args.Slot == slotName)
                continue;
            if (_inventory.TryGetSlotEntity(args.Equipee, slotName, out var existingItem))
            {
                if (!ent.Comp.Force)
                {
                    Rollback(args.Equipee, equipped, forcedOff, container);
                    return;
                }

                _inventory.TryUnequip(args.Equipee, slotName, predicted: true, silent: true);
                forcedOff[slotName] = existingItem.Value;
            }

            var existing = container.ContainedEntities
                .FirstOrDefault(e => MetaData(e).EntityPrototype?.ID == proto.Id);

            EntityUid item;
            if (existing != default)
            {
                _container.Remove(existing, container);
                item = existing;
            }
            else
            {
                item = Spawn(proto, Transform(ent).Coordinates);
            }

            if (!_inventory.TryEquip(args.Equipee, item, slotName, predicted: true, silent: true))
            {
                _container.Insert(item, container);
                Rollback(args.Equipee, equipped, forcedOff, container);
                return;
            }

            EnsureComp<UnremoveableComponent>(item);

            equipped[slotName] = item;
        }

        foreach (var (slotName, itemUid) in equipped)
            ent.Comp.SpawnedItems[slotName] = itemUid;

        foreach (var (slotName, itemUid) in forcedOff)
        {
            if (Exists(itemUid) && !_container.IsEntityInContainer(itemUid))
                _container.Insert(itemUid, container);
            ent.Comp.ForcedOffItems[slotName] = itemUid;
        }
    }

    private void OnGotUnequipped(Entity<MultiClothingComponent> ent, ref GotUnequippedEvent args)
    {
        if (!_net.IsServer)
            return;

        var container = _container.EnsureContainer<Container>(ent, ContainerId);

        foreach (var (slotName, itemUid) in ent.Comp.SpawnedItems)
        {
            if (_inventory.TryGetSlotEntity(args.Equipee, slotName, out var slotItem)
                && slotItem == itemUid)
            {
                RemComp<UnremoveableComponent>(itemUid);
                _inventory.TryUnequip(args.Equipee, slotName, predicted: true, silent: true);
            }

            if (Exists(itemUid))
                _container.Insert(itemUid, container);
        }

        ent.Comp.SpawnedItems.Clear();

        foreach (var (slotName, itemUid) in ent.Comp.ForcedOffItems)
        {
            if (!Exists(itemUid))
                continue;

            if (_container.IsEntityInContainer(itemUid))
                _container.Remove(itemUid, container);

            _inventory.TryEquip(args.Equipee, itemUid, slotName, predicted: true, silent: true);
        }

        ent.Comp.ForcedOffItems.Clear();
    }

    private void Rollback(
    EntityUid equipee,
    Dictionary<string, EntityUid> equipped,
    Dictionary<string, EntityUid> forcedOff,
    Container container)
    {
        foreach (var (slotName, itemUid) in equipped)
        {
            RemComp<UnremoveableComponent>(itemUid);
            _inventory.TryUnequip(equipee, slotName, predicted: true, silent: true);
            if (Exists(itemUid))
                _container.Insert(itemUid, container);
        }

        foreach (var (slotName, itemUid) in forcedOff)
        {
            if (Exists(itemUid))
                _inventory.TryEquip(equipee, itemUid, slotName, predicted: true, silent: true);
        }
    }
}