using Robust.Shared.Audio;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandSonicJackhammerComponent : Component
{
    [DataField]
    public int BurrowRange = 10;

    [DataField]
    public float BurrowStepDelay = 0.08f;

    [DataField]
    public int BurrowYieldMultiplier = 1;

    [DataField]
    public string BurrowPulsePrototype = "EffectGravityPulse";

    [DataField]
    public SoundSpecifier BurrowSound = new SoundPathSpecifier("/Audio/Items/drill_hit.ogg");
}
