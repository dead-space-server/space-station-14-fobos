// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Maths;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.DeadSpace.Sandevistan;

[RegisterComponent]
public sealed partial class SandevistanImplantComponent : Component
{
    [DataField]
    public float Duration = 16f;

    [DataField]
    public float SoftcapTime = 8f;

    [DataField]
    public float MovementSpeedModifier = 1.7f;

    [DataField]
    public float AttackRateModifier = 1.35f;

    [DataField]
    public float OverloadInterval = 1f;

    [DataField]
    public float OverloadStaminaDamage = 9f;

    [DataField]
    public DamageSpecifier OverloadDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 4 },
        },
    };

    [DataField]
    public float InitialJitterProgress = 0.2f;

    [DataField]
    public int MaxJitterHits = 5;

    [DataField]
    public float MaxJitterAmplitude = 5f;

    [DataField]
    public float MaxJitterFrequency = 30f;

    [DataField]
    public float JitterLerpRate = 5f;

    [DataField]
    public float JitterRefreshTime = 0.35f;

    [DataField]
    public float AfterimageInterval = 0.015f;

    [DataField]
    public float AfterimageMinDistance = 0.08f;

    [DataField]
    public float AfterimageLifetime = 0.35f;

    [DataField]
    public Color AfterimageColor = Color.FromHex("#c5ecff99");

    [DataField]
    public string AfterimageFallbackEffect = "MantisDodgeEffect";

    [DataField]
    public LocId? Popup = "sandevistan-implant-activated";
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class ActiveSandevistanComponent : Component
{
    [DataField]
    public EntityUid? SourceImplant;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan EndTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan SoftcapTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextOverloadTime;

    [DataField, AutoNetworkedField]
    public float MovementSpeedModifier = 1.7f;

    [DataField, AutoNetworkedField]
    public float AttackRateModifier = 1.35f;

    [DataField]
    public float OverloadInterval = 1f;

    [DataField]
    public float OverloadStaminaDamage = 9f;

    [DataField]
    public DamageSpecifier OverloadDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 4 },
        },
    };

    [DataField]
    public float JitterCurrentProgress;

    [DataField]
    public float JitterTargetProgress = 0.2f;

    [DataField]
    public float InitialJitterProgress = 0.2f;

    [DataField]
    public int JitterHits;

    [DataField]
    public int MaxJitterHits = 5;

    [DataField]
    public float MaxJitterAmplitude = 5f;

    [DataField]
    public float MaxJitterFrequency = 30f;

    [DataField]
    public float JitterLerpRate = 5f;

    [DataField]
    public float JitterRefreshTime = 0.35f;

    [DataField, AutoNetworkedField]
    public float AfterimageInterval = 0.015f;

    [DataField, AutoNetworkedField]
    public float AfterimageMinDistance = 0.08f;

    [DataField, AutoNetworkedField]
    public float AfterimageLifetime = 0.35f;

    [DataField, AutoNetworkedField]
    public Color AfterimageColor = Color.FromHex("#c5ecff99");

    [DataField, AutoNetworkedField]
    public string AfterimageFallbackEffect = "MantisDodgeEffect";
}
