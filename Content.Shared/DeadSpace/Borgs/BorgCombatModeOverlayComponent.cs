using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Borgs;

[RegisterComponent, NetworkedComponent]
public sealed partial class BorgCombatModeOverlayComponent : Component
{
}

[Serializable, NetSerializable]
public enum BorgCombatModeVisuals : byte
{
    Combat,
}
