using Content.Shared.Chasm;
using Content.Shared.DeadSpace.Lavaland.Components;
using Content.Shared.Inventory;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared.DeadSpace.Lavaland;

public sealed partial class SharedLavalandJaunterSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChasmFallingAttemptEvent>(OnChasmFallingAttempt);
    }

    private void OnChasmFallingAttempt(ChasmFallingAttemptEvent args)
    {
        if (!_net.IsClient ||
            args.Cancelled ||
            !_timing.IsFirstTimePredicted ||
            !TryFindEquippedJaunter(args.Tripper, out _))
        {
            return;
        }

        args.Cancel();
    }

    public bool TryFindEquippedJaunter(EntityUid user, out Entity<LavalandJaunterComponent> jaunter)
    {
        jaunter = default;

        foreach (var item in _inventory.GetHandOrInventoryEntities(user))
        {
            if (!TryComp<LavalandJaunterComponent>(item, out var component))
                continue;

            jaunter = (item, component);
            return true;
        }

        return false;
    }
}
