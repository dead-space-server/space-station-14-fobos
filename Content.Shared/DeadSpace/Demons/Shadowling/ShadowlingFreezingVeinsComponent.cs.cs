using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingFreezingVeinsComponent : Component
{
    [DataField] public EntProtoId ActionFreezingVeins = "ActionShadowlingFreezingVeins";
    [DataField] public EntityUid? ActionFreezingVeinsEntity;

    [DataField] public int RequiredSlaves = 5;
    [DataField] public float DamageCold = 30f;
    [DataField] public float TemperatureSet = 153.15f;
}

public sealed partial class ShadowlingFreezingVeinsEvent : EntityTargetActionEvent {}
