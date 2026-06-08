using Content.Shared.DeadSpace.Lavaland.Components;
using Content.Shared.Movement.Components;
using Content.Shared.StepTrigger.Components;
using Robust.Shared.Physics.Events;

namespace Content.Shared.StepTrigger.Systems;

public sealed class StepTriggerCollisionFilterSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StepTriggerCollisionFilterComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnPreventCollide(Entity<StepTriggerCollisionFilterComponent> ent, ref PreventCollideEvent args)
    {
        // DS14-Start: avoid persistent hazard-tile sensor contacts with entities that cannot trigger them.
        if ((ent.Comp.IgnoreCanMoveInAir && HasComp<CanMoveInAirComponent>(args.OtherEntity)) ||
            (ent.Comp.IgnoreLavalandChasmImmune && HasComp<LavalandChasmImmuneComponent>(args.OtherEntity)) ||
            (ent.Comp.IgnoreLavalandFauna && HasComp<LavalandFaunaComponent>(args.OtherEntity)))
        {
            args.Cancelled = true;
        }
        // DS14-End
    }
}
