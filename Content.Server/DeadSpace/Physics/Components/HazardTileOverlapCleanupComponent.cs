using Content.Server.DeadSpace.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Physics.Components;

[RegisterComponent, Access(typeof(HazardTileOverlapCleanupSystem))]
public sealed partial class HazardTileOverlapCleanupComponent : Component
{
    [DataField]
    public List<EntProtoId> HazardPrototypes = ["FloorLavaEntity"];
}
