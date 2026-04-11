using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingBlinkComponent : Component
{
    [DataField] public EntProtoId ActionBlink = "ActionShadowlingBlink";
    [DataField] public EntityUid? ActionBlinkEntity;
}

public sealed partial class ShadowlingBlinkEvent : EntityTargetActionEvent {}
