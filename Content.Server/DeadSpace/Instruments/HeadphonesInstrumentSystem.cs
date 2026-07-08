// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Instruments;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Server.Actions;

namespace Content.Server.DeadSpace.Instruments;

public sealed class HeadphonesInstrumentSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeadphonesInstrumentComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<HeadphonesInstrumentComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<HeadphonesInstrumentComponent, ItemToggledEvent>(OnItemToggled);

        SubscribeLocalEvent<NoiseCancellingWearerComponent, ToggleNoiseCancellingActionEvent>(OnActionToggled);
    }

    private void OnEquipped(EntityUid uid, HeadphonesInstrumentComponent comp, ref GotEquippedEvent args)
    {
        var wearerComp = EnsureComp<NoiseCancellingWearerComponent>(args.Equipee);
        wearerComp.HeadphonesUid = uid;

        _actions.AddAction(args.Equipee, ref comp.ActionEntity, comp.NoiseCancellingAction);

        if (TryComp<ItemToggleComponent>(uid, out var toggle) && toggle.Activated)
            ApplyNoiseCancelling(args.Equipee);
    }

    private void OnUnequipped(EntityUid uid, HeadphonesInstrumentComponent comp, ref GotUnequippedEvent args)
    {
        RemComp<NoiseCancellingWearerComponent>(args.Equipee);

        _actions.RemoveAction(args.Equipee, comp.ActionEntity);
        comp.ActionEntity = null;

        RemoveNoiseCancelling(args.Equipee);
    }

    private void OnActionToggled(EntityUid uid, NoiseCancellingWearerComponent comp, ToggleNoiseCancellingActionEvent args)
    {
        if (!Exists(comp.HeadphonesUid))
            return;

        _itemToggle.Toggle(comp.HeadphonesUid, uid);
        args.Handled = true;
    }

    private void OnItemToggled(EntityUid uid, HeadphonesInstrumentComponent comp, ref ItemToggledEvent args)
    {
        var wearer = Transform(uid).ParentUid;
        if (!wearer.Valid)
            return;

        if (args.Activated)
            ApplyNoiseCancelling(wearer);
        else
            RemoveNoiseCancelling(wearer);
    }

    private void ApplyNoiseCancelling(EntityUid wearer)
    {
        _popup.PopupEntity("Режим шумоподавления включен.", wearer, wearer);
    }

    private void RemoveNoiseCancelling(EntityUid wearer)
    {
        _popup.PopupEntity("Режим шумоподавления выключен.", wearer, wearer);
    }
}
