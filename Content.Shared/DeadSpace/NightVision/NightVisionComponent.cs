using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.NightVision;

[NetworkedComponent]
public abstract partial class SharedNightVisionComponent : Component
{
    [DataField]
    public bool IsNightVision;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public Color Color = new Color(80f / 255f, 220f / 255f, 70f / 255f, 0.1f);

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? ActivateSound = null;

    [DataField]
    public float? Duration = null;

    [DataField]
    public bool Animation = false;

    [DataField]  // DS14-Start
    public float? Desaturation = null;  // null = full greyscale, 0.0-1.0 = partial desaturation  // DS14-End
}

public sealed partial class ToggleNightVisionActionEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed class NightVisionComponentState : ComponentState
{
    public Color Color;
    public bool IsNightVision;
    public uint LastToggleTick;
    public SoundSpecifier? ActivateSound;
    public bool Animation;
    public float? Duration;
    public float? Desaturation;  // DS14

    public NightVisionComponentState(
        Color color,
        bool isNightVision,
        uint lastToggleTick,
        SoundSpecifier? activateSound,
        bool animation,
        float? duration,
        float? desaturation = null)  // DS14
    {
        Color = color;
        IsNightVision = isNightVision;
        LastToggleTick = lastToggleTick;
        ActivateSound = activateSound;
        Animation = animation;
        Duration = duration;
        Desaturation = desaturation;  // DS14
    }
}
