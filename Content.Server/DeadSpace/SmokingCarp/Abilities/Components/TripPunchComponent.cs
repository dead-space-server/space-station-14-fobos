using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.DeadSpace.SmokingCarp.Abilities.Components;

[RegisterComponent]
public sealed partial class TripPunchComponent : Component
{
    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(1.0);

    [DataField]
    public float Range = 1.0f;

    [DataField]
    public EntProtoId? SelfEffect = "EffectTripPunchCarp";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_DeadSpace/SmokingCarp/sound_items_weapons_slam.ogg");

    [DataField]
    public EntityUid? ActionTripPunchCarpEntity;

    [DataField("actionTripPunchCarp", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionTripPunchCarpAtack = "ActionTripPunchCarp";
}
