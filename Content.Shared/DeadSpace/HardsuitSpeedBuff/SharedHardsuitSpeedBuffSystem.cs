using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;

namespace Content.Shared.DeadSpace.HardsuitSpeedBuff;

public sealed class SharedHardsuitSpeedBuffSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HardsuitSpeedBuffComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
    }

    private void OnRefreshMovementSpeedModifiers(Entity<HardsuitSpeedBuffComponent> ent, ref InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (!ent.Comp.Activated)
            return;

        args.Args.ModifySpeed(ent.Comp.WalkModifier, ent.Comp.SprintModifier);
    }
}
