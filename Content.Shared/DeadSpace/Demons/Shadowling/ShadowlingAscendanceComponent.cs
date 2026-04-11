using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAscendanceComponent : Component
{
    [DataField] public EntProtoId ActionAscendance = "ActionShadowlingAscendance";
    [DataField] public EntityUid? ActionAscendanceEntity;

    [DataField] public int RequiredSlaves = 15;
    [DataField] public float Duration = 14f;
}

public sealed partial class ShadowlingAscendanceEvent : InstantActionEvent {}
