using Robust.Shared.Audio;

namespace Content.Server.DeadSpace.GPS.Components;

[RegisterComponent]
public sealed partial class LavalandGpsTrackerComponent : Component
{
    [DataField]
    public float DetectionRange = 100f;

    [DataField]
    public SoundSpecifier BeepSound = new SoundPathSpecifier("/Audio/Items/locator_beep.ogg");

    [DataField]
    public float MinBeepInterval = 0.25f;

    [DataField]
    public float MaxBeepInterval = 1.5f;

    public TimeSpan NextBeepTime;
}
