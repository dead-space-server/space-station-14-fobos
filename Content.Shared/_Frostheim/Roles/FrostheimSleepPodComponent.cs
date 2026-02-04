using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Frostheim.Roles;

[RegisterComponent, NetworkedComponent]
public sealed partial class FrostheimSleepPodComponent : Component
{
    [DataField]
    public string ContainerId = "frostPod";
}

[Serializable, NetSerializable]
public enum FrostheimSleepPodVisuals : byte
{
    Occupied
}
