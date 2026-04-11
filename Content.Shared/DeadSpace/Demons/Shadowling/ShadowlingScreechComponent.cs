using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingScreechComponent : Component
{
    [DataField] public EntProtoId ActionScreech = "ActionShadowlingScreech";
    [DataField] public EntityUid? ActionScreechEntity;

    [DataField] public float Range = 5f;
    [DataField] public float StunDuration = 4f;

    [DataField] public int RequiredSlaves = 3;
}

public sealed partial class ShadowlingScreechEvent : InstantActionEvent {}
