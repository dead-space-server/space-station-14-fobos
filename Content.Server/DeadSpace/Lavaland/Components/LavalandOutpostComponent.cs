using Content.Shared.DeadSpace.Lavaland;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandOutpostComponent : Component
{
    [DataField]
    public EntityUid Station;

    [DataField]
    public EntityUid Map;

    [DataField]
    public ProtoId<LavalandPlanetPrototype> Planet;
}
