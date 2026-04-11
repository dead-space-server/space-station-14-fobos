using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingSlaveComponent : Component
{
    [DataField] 
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "ShadowlingSlaveFaction";

    [ViewVariables] 
    public EntityUid? Master;
}