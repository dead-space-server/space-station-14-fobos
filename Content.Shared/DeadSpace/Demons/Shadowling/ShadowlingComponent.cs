using Content.Shared.Damage;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingComponent : Component
{
    [DataField("passiveHealing")] public DamageSpecifier PassiveHealing = new();
    [DataField("healingInterval")] public float HealingInterval = 1.0f;
    [DataField("speedMultiplier")] public float SpeedMultiplier = 1.5f;
    [DataField("threshold")] public float Threshold = 0.5f;
    [DataField] public EntProtoId ActionBlink = "ActionShadowlingBlink";
    [DataField] public EntityUid? ActionBlinkEntity;
    
    [DataField] public EntProtoId ActionAscendance = "ActionShadowlingAscendance";
    
    [ViewVariables] public EntityUid? ActionAscendanceEntity;

    [ViewVariables] public float Accumulator = 0f;
    [ViewVariables] public bool IsInDarkness = false;
}

public sealed partial class ShadowlingAscendanceEvent : InstantActionEvent {}
