using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingDarkMindComponent : Component
{
    [DataField] 
    public EntProtoId ActionDarkMind = "ActionShadowlingDarkMind";

    [DataField]
    public EntityUid? ActionDarkMindEntity;
}

public sealed partial class ShadowlingDarkMindEvent : InstantActionEvent {}
