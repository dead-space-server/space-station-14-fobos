using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShadowlingRevealComponent : Component
{
    [DataField] public EntProtoId ActionReveal = "ActionShadowlingReveal";
    [DataField, AutoNetworkedField] public EntityUid? ActionRevealEntity;
    [DataField] public float Duration = 14f;
}

public sealed partial class ShadowlingRevealEvent : InstantActionEvent {}

[Serializable, NetSerializable]
public sealed partial class ShadowlingRevealDoAfterEvent : SimpleDoAfterEvent {}
