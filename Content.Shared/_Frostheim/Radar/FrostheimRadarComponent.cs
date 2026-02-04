using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim.Radar;

[RegisterComponent, NetworkedComponent]
public sealed partial class FrostheimRadarComponent : Component
{
    [DataField]
    public float MaxRange = 200f;

    [DataField]
    public float UpdateInterval = 0.3f;

    [ViewVariables]
    public EntityUid? ActiveUser;
}
