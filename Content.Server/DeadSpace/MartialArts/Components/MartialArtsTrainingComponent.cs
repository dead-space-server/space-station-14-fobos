using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.MartialArts.Components;

public readonly record struct ArkalyseParams(
    float StaminaDamageMuteAtack = 25.0f,
    float ParalyzeTimeMuteAtack = 10.0f,
    int HitDamageForDamageAtack = 15,
    int HitDamageForMuteAtack = 5,
    float ParalyzeTimeStunAtack = 0.5f,
    bool IgnoreResist = true,
    string DamageTypeForDamageAtack = "Piercing",
    string DamageTypeForMuteAtack = "Blunt",
    EntProtoId? EffectPunchForDamageAtack = null,
    EntProtoId? EffectPunchForStunAtack = null,
    SoundSpecifier? HitSoundForDamageAtack = null,
    SoundSpecifier? HitSoundForStunAtack = null
);

public readonly record struct SmokingCarpParams(
    float StaminaDamageSmokePunch = 5.0f,
    int HitDamageForSmokePunch = 5,
    int HitDamageForPowerPunch = 30,
    bool IgnoreResist = true,
    string DamageTypeForPowerPunch = "Slash",
    string DamageTypeForSmokePunch = "Blunt",
    float PushStrength = 300.0f,
    float MaxPushDistance = 5.0f,
    EntProtoId? EffectPowerPunch = null,
    EntProtoId? EffectSmokePunch = null,
    SoundSpecifier? HitSoundForPowerPunch = null,
    SoundSpecifier? HitSoundForSmokePunch = null,
    List<LocId>? PackMessageOnHit = null
);

[RegisterComponent]
public sealed partial class MartialArtsTrainingCarpComponent : Component
{
    [DataField]
    public float AddAtackRate = 1.15f;

    [DataField]
    public MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.SmokingCarp;

    [DataField]
    public EntProtoId? ItemAfterLerning;

    [DataField]
    public SmokingCarpParams Params { get; set; }
}

[RegisterComponent]
public sealed partial class MartialArtsTrainingArkalyseComponent : Component
{
    [DataField]
    public float AddAtackRate = 1.1f;

    [DataField]
    public MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.Arkalyse;

    [DataField]
    public EntProtoId? ItemAfterLerning;

    [DataField]
    public ArkalyseParams Params { get; set; }
}

public enum MartialArtsForms
{
    Arkalyse,
    SmokingCarp,
}
