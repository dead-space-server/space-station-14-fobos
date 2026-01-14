using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.MartialArts.Components;

[DataField]
public readonly record struct ArkalyseParams(
    float StaminaDamageMuteAtack,
    float ParalyzeTimeMuteAtack,
    int HitDamageForDamageAtack,
    int HitDamageForMuteAtack,
    float ParalyzeTimeStunAtack,
    bool IgnoreResist,
    string DamageTypeForDamageAtack = "Slash",
    string DamageTypeForMuteAtack = "Blunt",
    EntProtoId? EffectPunchForDamageAtack,
    EntProtoId? EffectPunchForStunAtack,
    SoundSpecifier? HitSoundForDamageAtack,
    SoundSpecifier? HitSoundForStunAtack
);

[DataField]
public readonly record struct SmokingCarpParams(
    float StaminaDamageSmokePunch,
    int HitDamageForSmokePunch,
    int HitDamageForPowerPunch,
    bool IgnoreResist,
    string DamageTypeForPowerPunch = "Slash",
    string DamageTypeForSmokePunch = "Blunt",
    float PushStrength,
    float MaxPushDistance,
    EntProtoId? EffectPowerPunch,
    EntProtoId? EffectSmokePunch,
    SoundSpecifier? HitSoundForPowerPunch,
    SoundSpecifier? HitSoundForSmokePunch,
    List<LocId> PackMessageOnHit = new()
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
