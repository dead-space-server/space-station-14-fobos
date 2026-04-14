// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeadSpace.Kitchen.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Kitchen;

public sealed class SharedPlateSystem : EntitySystem
{
    private const int EatAltVerbPriority = 10;

    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IngestionSystem _ingestion = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly Dictionary<ProtoId<ItemSizePrototype>, EntityWhitelist> _sizeWhitelistCache = new();

    public override void Initialize()
    {
        base.Initialize();

        if (!_net.IsClient)
            SubscribeLocalEvent<PlateComponent, MapInitEvent>(OnPlateMapInit);
        SubscribeLocalEvent<PlateComponent, UseInHandEvent>(OnUseInHand, before: [typeof(ItemSlotsSystem)]);
        SubscribeLocalEvent<PlateComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerbs);
        SubscribeLocalEvent<HandsComponent, AccessibleOverrideEvent>(OnAccessibleOverride);
        SubscribeLocalEvent<HandsComponent, InRangeOverrideEvent>(OnInRangeOverride);
    }

    private void OnPlateMapInit(Entity<PlateComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<ItemSlotsComponent>(ent.Owner, out var itemSlots) ||
            !_itemSlots.TryGetSlot(ent.Owner, ent.Comp.SlotId, out var slot, itemSlots))
        {
            return;
        }

        slot.Whitelist = GetSizeWhitelist(ent.Comp.MaxItemSize);
        Dirty(ent.Owner, itemSlots);
    }

    private void OnUseInHand(Entity<PlateComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryGetEdibleContent(ent.Owner, ent.Comp, out _))
            return;

        TryUsePlateContent(ent.Owner, ent.Comp, args.User);
        args.Handled = true;
    }

    private void OnGetAlternativeVerbs(Entity<PlateComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (args.Hands == null ||
            !args.CanAccess ||
            !args.CanInteract ||
            _hands.IsHolding((args.User, args.Hands), ent.Owner))
        {
            return;
        }

        var content = _itemSlots.GetItemOrNull(ent.Owner, ent.Comp.SlotId);
        if (content == null || !TryGetIngestionVerb(args.User, content.Value, out var verb) || verb == null)
            return;

        var user = args.User;
        if (verb.Priority < EatAltVerbPriority)
            verb.Priority = EatAltVerbPriority;
        verb.Act = () => TryUsePlateContent(ent.Owner, ent.Comp, user);
        args.Verbs.Add(verb);
    }

    private bool TryUsePlateContent(EntityUid plate, PlateComponent component, EntityUid user)
    {
        if (!TryGetEdibleContent(plate, component, out var content))
            return false;

        return _ingestion.TryIngest(user, user, content.Value);
    }

    private bool TryGetIngestionVerb(EntityUid user, EntityUid content, [NotNullWhen(true)] out AlternativeVerb? verb)
    {
        verb = null;
        var type = _ingestion.GetEdibleType((content, CompOrNull<EdibleComponent>(content)));
        return type != null && _ingestion.TryGetIngestionVerb(user, content, type.Value, out verb);
    }

    private bool TryGetEdibleContent(EntityUid plate, PlateComponent component, [NotNullWhen(true)] out EntityUid? content)
    {
        content = _itemSlots.GetItemOrNull(plate, component.SlotId);
        return content != null && _ingestion.GetEdibleType((content.Value, CompOrNull<EdibleComponent>(content.Value))) != null;
    }

    private void OnAccessibleOverride(Entity<HandsComponent> ent, ref AccessibleOverrideEvent args)
    {
        if (!TryGetPlate(args.Target, out var plate) || !_interaction.CanAccess(ent.Owner, plate.Value))
            return;

        args.Handled = true;
        args.Accessible = true;
    }

    private void OnInRangeOverride(Entity<HandsComponent> ent, ref InRangeOverrideEvent args)
    {
        if (!TryGetPlate(args.Target, out var plate) || !_interaction.InRangeUnobstructed(ent.Owner, plate.Value))
            return;

        args.Handled = true;
        args.InRange = true;
    }

    private bool TryGetPlate(EntityUid target, [NotNullWhen(true)] out EntityUid? plate)
    {
        plate = null;

        if (!_containers.TryGetContainingContainer(target, out var container) ||
            !TryComp(container.Owner, out PlateComponent? plateComp) ||
            container.ID != plateComp.SlotId)
        {
            return false;
        }

        plate = container.Owner;
        return true;
    }

    private EntityWhitelist GetSizeWhitelist(ProtoId<ItemSizePrototype> maxItemSize)
    {
        if (_sizeWhitelistCache.TryGetValue(maxItemSize, out var whitelist))
            return whitelist;

        var maxSize = _item.GetSizePrototype(maxItemSize);
        whitelist = new EntityWhitelist
        {
            Sizes = _prototype.EnumeratePrototypes<ItemSizePrototype>()
                .Where(size => size <= maxSize)
                .OrderBy(size => size.Weight)
                .Select(size => (ProtoId<ItemSizePrototype>) size.ID)
                .ToList(),
        };

        _sizeWhitelistCache[maxItemSize] = whitelist;
        return whitelist;
    }
}
