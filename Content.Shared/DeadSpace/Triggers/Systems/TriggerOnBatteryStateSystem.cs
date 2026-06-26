using Content.Shared.DeadSpace.Triggers.Components;
using Content.Shared.Trigger;
using Content.Shared.Power;

namespace Content.Shared.DeadSpace.Triggers.Systems;

public sealed class TriggerOnBatteryStateSystem : TriggerOnXSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnBatteryStateComponent, BatteryStateChangedEvent>(OnBatteryStateChanged);
    }

    private void OnBatteryStateChanged(Entity<TriggerOnBatteryStateComponent> ent, ref BatteryStateChangedEvent args)
    {
        if (ent.Comp.Triggered)
            return;

        if (args.NewState == ent.Comp.State)
        {
            Trigger.Trigger(ent.Owner, null, ent.Comp.KeyOut);
            ent.Comp.Triggered = true;
        }
    }
}
