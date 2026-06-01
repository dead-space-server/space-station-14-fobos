using Content.Server.Worldgen.Systems;

namespace Content.Server.Worldgen.Components;

// DS14-Start: delay deletion of unloaded worldgen chunks to avoid churn.
[RegisterComponent]
[Access(typeof(WorldControllerSystem))]
public sealed partial class ChunkEvictionComponent : Component
{
    [ViewVariables]
    public TimeSpan EvictAt;
}
// DS14-End
