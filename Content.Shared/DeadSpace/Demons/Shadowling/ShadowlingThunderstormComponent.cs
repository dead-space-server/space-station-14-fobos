using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingThunderstormComponent : Component
{
    [DataField] public EntProtoId ActionThunderstorm = "ActionShadowlingThunderstorm";
    [DataField] public EntityUid? ActionThunderstormEntity;

    [DataField] public float Range = 10f;
    [DataField] public int MaxTargets = 5;
    [DataField] public float StunDuration = 5f;
    [DataField] public string LightningPrototype = "Lightning";
}

public sealed partial class ShadowlingThunderstormEvent : EntityTargetActionEvent {}
