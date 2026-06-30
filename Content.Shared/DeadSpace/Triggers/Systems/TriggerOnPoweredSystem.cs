using Content.Shared.DeadSpace.Triggers.Components;
using Content.Shared.Trigger;
using Content.Shared.Power;

namespace Content.Shared.DeadSpace.Triggers.Systems;

public sealed class TriggerOnPoweredSystem : TriggerOnXSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnPoweredComponent, PowerChangedEvent>(HandlePowerChange);
    }

    private void HandlePowerChange(Entity<TriggerOnPoweredComponent> ent, ref PowerChangedEvent args)
    {
        if (args.Powered == true && ent.Comp.TriggerIfGetPower)
        {
            Trigger.Trigger(ent.Owner, null, ent.Comp.KeyOut);
        }

        if (args.Powered == false && ent.Comp.TriggerIfLosePower)
        {
            Trigger.Trigger(ent.Owner, null, ent.Comp.KeyOut);
        }
    }
}
