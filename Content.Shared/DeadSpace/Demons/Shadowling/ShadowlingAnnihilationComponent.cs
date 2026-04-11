using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAnnihilationComponent : Component
{
    [DataField] 
    public EntProtoId ActionAnnihilation = "ActionShadowlingAnnihilation";

    [DataField] 
    public EntityUid? ActionAnnihilationEntity;
}

public sealed partial class ShadowlingAnnihilationEvent : EntityTargetActionEvent {}
