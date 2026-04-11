using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingRecruitComponent : Component
{
    [DataField] public EntProtoId ActionRecruit = "ActionShadowlingRecruit";
    [DataField] public EntityUid? ActionRecruitEntity;

    [DataField] public float Duration = 8f;

    [ViewVariables] public int CurrentSlaves = 0;
}

public sealed partial class ShadowlingRecruitEvent : EntityTargetActionEvent {}

[Serializable, NetSerializable]
public sealed partial class ShadowlingRecruitDoAfterEvent : SimpleDoAfterEvent {}
