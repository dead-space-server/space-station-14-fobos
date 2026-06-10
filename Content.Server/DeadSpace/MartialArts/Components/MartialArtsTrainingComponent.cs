// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.MartialArts.Components;

[DataDefinition]
public sealed partial class ArkalyseParams // Список переменных, которые будут передаваться при использовании предмета
{
    [DataField]
    public float StaminaDamageMuteAtack = 25.0f;

    [DataField]
    public TimeSpan ParalyzeTimeMuteAtack = TimeSpan.FromSeconds(10);

    [DataField]
    public int HitDamageForDamageAtack = 15;

    [DataField]
    public int HitDamageForMuteAtack = 5;

    [DataField]
    public float ParalyzeTimeStunAtack = 0.5f;

    [DataField]
    public bool IgnoreResist = true;

    [DataField]
    public string DamageTypeForDamageAtack = "Piercing";

    [DataField]
    public string DamageTypeForMuteAtack = "Blunt";

    [DataField]
    public EntProtoId? EffectPunchForDamageAtack;

    [DataField]
    public EntProtoId? EffectPunchForStunAtack;

    [DataField]
    public SoundSpecifier? HitSoundForDamageAtack;

    [DataField]
    public SoundSpecifier? HitSoundForStunAtack;
}

[DataDefinition]
public sealed partial class SmokingCarpParams // Список переменных, которые будут передаваться при использовании предмета
{
    [DataField]
    public float StaminaDamageSmokePunch = 35.0f;

    [DataField]
    public int HitDamageForSmokePunch = 5;

    [DataField]
    public int HitDamageForPowerPunch = 30;

    [DataField]
    public bool IgnoreResist = true;

    [DataField]
    public string DamageTypeForPowerPunch = "Slash";

    [DataField]
    public string DamageTypeForSmokePunch = "Blunt";

    [DataField]
    public float PushStrength = 300.0f;

    [DataField]
    public float MaxPushDistance = 5.0f;

    [DataField]
    public EntProtoId? EffectPowerPunch;

    [DataField]
    public EntProtoId? EffectSmokePunch;

    [DataField]
    public SoundSpecifier? HitSoundForPowerPunch;

    [DataField]
    public SoundSpecifier? HitSoundForSmokePunch;

    [DataField]
    public List<LocId>? PackMessageOnHit;
}

[RegisterComponent]
public sealed partial class MartialArtsTrainingCarpComponent : Component
{
    [DataField]
    public float AddAtackRate = 1.15f; // Меняет скорость атаки пользователя

    [DataField]
    public EntProtoId? ItemAfterLerning; // Прототип объекта, в который будет преобразован предмет при использовании

    [DataField]
    public List<SmokingCarpParams> Params { get; set; } = new(); // Хранение параметров из SmokingCarpParams
}

[RegisterComponent]
public sealed partial class MartialArtsTrainingArkalyseComponent : Component
{
    [DataField]
    public float AddAtackRate = 1.1f; // Меняет скорость атаки пользователя

    [DataField]
    public EntProtoId? ItemAfterLerning; // Прототип объекта, в который будет преобразован предмет при использовании

    [DataField]
    public List<ArkalyseParams> Params { get; set; } = new(); // Хранение параметров из ArkalyseParams
}