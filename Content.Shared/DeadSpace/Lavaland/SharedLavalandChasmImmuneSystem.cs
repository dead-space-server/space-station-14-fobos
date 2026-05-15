using Content.Shared.Chasm;
using Content.Shared.DeadSpace.Lavaland.Components;

namespace Content.Shared.DeadSpace.Lavaland;

public sealed class SharedLavalandChasmImmuneSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LavalandChasmImmuneComponent, ChasmFallingAttemptEvent>(OnChasmFallingAttempt);
    }

    private void OnChasmFallingAttempt(Entity<LavalandChasmImmuneComponent> ent, ref ChasmFallingAttemptEvent args)
    {
        args.Cancel();
    }
}
