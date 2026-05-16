using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.DeadSpace.Lavaland.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LavalandJaunterComponent : Component
{
    [DataField]
    public float SearchRadius = 24f;

    [DataField]
    public int SearchAttempts = 96;

    [DataField]
    public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");

    [DataField]
    public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextAutomaticUse = TimeSpan.Zero;

    [DataField]
    public TimeSpan AutomaticUseCooldown = TimeSpan.FromSeconds(1);
}
