// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.ThermalVision;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Server.DeadSpace.ThermalVision;

public sealed class ThermalVisionSystem : EntitySystem
{
    public const SlotFlags ValidSlots = SlotFlags.EYES;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ThermalVisionComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<ThermalVisionComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotEquipped(EntityUid entity, ThermalVisionComponent comp, ref GotEquippedEvent args)
    {
        if ((args.SlotFlags & ValidSlots) == 0)
            return;

        if (HasComp<ThermalVisionActiveComponent>(args.Equipee))
            return;

        var activeComp = new ThermalVisionActiveComponent
        {
            ActivateSound = comp.ActivateSound,
            ActivateSoundOff = comp.ActivateSoundOff,
            Animation = comp.Animation
        };
        comp.HasThermalVision = true;

        AddComp(args.Equipee, activeComp);
    }

    private void OnGotUnequipped(EntityUid entity, ThermalVisionComponent comp, ref GotUnequippedEvent args)
    {
        if (comp.HasThermalVision && HasComp<ThermalVisionActiveComponent>(args.Equipee))
            RemComp<ThermalVisionActiveComponent>(args.Equipee);
    }
}