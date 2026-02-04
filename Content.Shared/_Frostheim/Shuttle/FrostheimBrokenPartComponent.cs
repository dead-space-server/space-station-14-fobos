using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Frostheim.Shuttle;

[RegisterComponent, NetworkedComponent]
public sealed partial class FrostheimBrokenPartComponent : Component
{
    [DataField]
    public FrostheimPartType PartType = FrostheimPartType.Thruster;
}

[Serializable, NetSerializable]
public enum FrostheimPartType : byte
{
    Thruster,
    Gyroscope
}
