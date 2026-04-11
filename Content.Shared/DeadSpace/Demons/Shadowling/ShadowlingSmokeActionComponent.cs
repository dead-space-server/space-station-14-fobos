using Robust.Shared.GameStates;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingSmokeActionComponent : Component
{
    [DataField] public EntProtoId ActionSmoke = "ActionShadowlingSmoke";

    [DataField] public EntityUid? ActionSmokeEntity;

    /// <summary>
    /// Время существования дыма в секундах.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float SmokeDuration = 20f;

    /// <summary>
    /// Интенсивность (распространение) дыма.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int SmokeSpread = 30;
}

public sealed partial class ShadowlingSmokeActionEvent : InstantActionEvent {}
