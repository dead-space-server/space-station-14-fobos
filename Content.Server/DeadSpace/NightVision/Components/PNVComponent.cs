using System;

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Components.NightVision;

[RegisterComponent]
public sealed partial class PNVComponent : Component
{
    [DataField]
    public Color? Color = null;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool HasNightVision = false;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Animation = true;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId ActionToggleNightVision = "ActionToggleNightVision";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? ActivateSound = null;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float? Desaturation = null;

    [ViewVariables(VVAccess.ReadOnly)]
    public PreviousNightVisionState? PreviousNightVision;
}

public sealed class PreviousNightVisionState
{
    public Color Color;
    public SoundSpecifier? ActivateSound;
    public float? Duration;
    public bool Animation;
    public float? Desaturation;
    public EntProtoId ActionToggleNightVision;
    public EntityUid? ActionToggleNightVisionEntity;
    public TimeSpan? ActionCooldownRemaining;
    public bool? ActionWasTemporary;
}
