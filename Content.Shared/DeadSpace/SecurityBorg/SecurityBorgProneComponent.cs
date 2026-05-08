using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.Actions;

namespace Content.Shared.DeadSpace.SecurityBorg;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SecurityBorgProneComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionSecurityBorgProne";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public bool Enabled;

    [DataField]
    public float SpeedModifier = 0.7f;

    [DataField]
    public Vector2 HeadSlotProneOffsetDelta = new(0f, -0.24f);
}

[Serializable, NetSerializable]
public enum SecurityBorgProneVisuals : byte
{
    Prone,
}

public sealed partial class SecurityBorgProneActionEvent : InstantActionEvent;