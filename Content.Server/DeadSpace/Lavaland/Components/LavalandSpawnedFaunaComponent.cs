using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandSpawnedFaunaComponent : Component
{
    [DataField]
    public EntityUid Map;

    [DataField]
    public EntProtoId Prototype = default;

    [DataField]
    public Vector2i Sector;
}
