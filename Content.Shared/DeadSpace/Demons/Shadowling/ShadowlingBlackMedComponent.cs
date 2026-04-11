using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingBlackMedComponent : Component
{
    [DataField] public EntProtoId ActionBlackMed = "ActionShadowlingBlackMed";
    [DataField] public EntityUid? ActionBlackMedEntity;

    [DataField] public int RequiredSlaves = 9;
}

public sealed partial class ShadowlingBlackMedEvent : EntityTargetActionEvent {}
