using Content.Shared._Frostheim.Supermatter.Data;
using Content.Shared.Atmos;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim.Supermatter.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SupermatterComponent : Component
{
    [DataField("whitelist")] public EntityWhitelist Whitelist = new();

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("power")]
    public float Power;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("damage")]
    public float Damage;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("matterPower")]
    public float MatterPower;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("matterPowerConversion")]
    public float MatterPowerConversion = 10f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("gasEfficiency")]
    public float GasEfficiency = 0.15f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("heatThreshold")]
    public float HeatThreshold = 2500f;

    public EntityUid? AudioEntity;

    public SuperMatterSound? SmSound;

    [DataField("dustSound")]
    public SoundSpecifier DustSound = new SoundPathSpecifier("/Audio/_Frostheim/Supermatter/dust.ogg");

    [DataField("delamSound")]
    public SoundSpecifier DelamSound = new SoundPathSpecifier("/Audio/_Frostheim/Supermatter/delamming.ogg");

    [DataField("delamAlarm")]
    public SoundSpecifier DelamAlarm = new SoundPathSpecifier("/Audio/Machines/alarm.ogg");

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("dynamicheatResistance")]
    public float DynamicHeatResistance = 1;

    /// <summary>
    /// TG: REACTION_POWER_MODIFIER = 0.65
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("reactionpowerModefier")]
    public float ReactionPowerModifier = 0.65f;

    /// <summary>
    /// TG: THERMAL_RELEASE_MODIFIER = 4
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("thermalreleaseModifier")]
    public float ThermalReleaseModifier = 4f;

    /// <summary>
    /// TG: PLASMA_RELEASE_MODIFIER = 650
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("plasmareleaseModifier")]
    public float PlasmaReleaseModifier = 650f;

    /// <summary>
    /// TG: OXYGEN_RELEASE_MODIFIER = 340
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("oxygenreleaseModifier")]
    public float OxygenReleaseModifier = 340f;

    /// <summary>
    /// TG: GAS_HEAT_POWER_SCALING_COEFFICIENT = 1/6
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("heatPowerScaling")]
    public float HeatPowerScaling = 1f / 6f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("WarningPoint")]
    public float WarningPoint = 15;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("emergencyPoint")]
    public float EmergencyPoint = 80;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("yellTimer")]
    public float YellTimer = 30f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("yellAccumulator")]
    public float YellAccumulator = 30f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("yellDelam")]
    public float YellDelam = 5f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("damageupdateAccumulator")]
    public float DamageUpdateAccumulator;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("damageupdateTimer")]
    public float DamageUpdateTimer = 1f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("delamtimerAccumulator")]
    public float DelamTimerAccumulator;

    /// <summary>
    /// TG: SUPERMATTER_COUNTDOWN_TIME = 15 seconds
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("delamtimerTimer")]
    public int DelamTimerTimer = 15;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("speakaccumulator")]
    public float SpeakAccumulator = 5f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("atmosupdateAccumulator")]
    public float AtmosUpdateAccumulator;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("atmosupdateTimer")]
    public float AtmosUpdateTimer = 1f;

    /// <summary>
    /// TG: POWERLOSS_CUBIC_DIVISOR = 500
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("powerlossCubicDivisor")]
    public float PowerlossCubicDivisor = 500f;

    /// <summary>
    /// TG: POWERLOSS_LINEAR_RATE = 0.83
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("powerlossLinearRate")]
    public float PowerlossLinearRate = 0.83f;

    /// <summary>
    /// TG: MOLE_PENALTY_THRESHOLD = 1800 moles
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("molepenaltyThreshold")]
    public float MolePenaltyThreshold = 1800f;

    /// <summary>
    /// TG: POWER_PENALTY_THRESHOLD = 5000
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("powerPenaltyThreshold")]
    public float PowerPenaltyThreshold = 5000f;

    /// <summary>
    /// TG: HEAT_PENALTY_THRESHOLD = 40K above T0C (so ~313K total, damage starts at ~626K with resistance)
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("heatpenaltyThreshold")]
    public float HeatPenaltyThreshold = 40f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("damagearchived")]
    public float DamageArchived;

    /// <summary>
    /// TG: Heat damage divisor = 24000, Power = 40000, Moles = 3200
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("heatDamageDivisor")]
    public float HeatDamageDivisor = 6000f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("powerDamageDivisor")]
    public float PowerDamageDivisor = 10000f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("moleDamageDivisor")]
    public float MoleDamageDivisor = 800f;

    /// <summary>
    /// TG: Space damage = internal_energy * 0.000125, cap 1.0
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("spaceDamageMultiplier")]
    public float SpaceDamageMultiplier = 0.005f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("maxspaceexposureDamage")]
    public float MaxSpaceExposureDamage = 2f;

    /// <summary>
    /// TG: Healing divisor = 6000, cap -0.1
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("healingDivisor")]
    public float HealingDivisor = 6000f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("maxHealingPerTick")]
    public float MaxHealingPerTick = 0.02f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("explosionPoint")]
    public int ExplosionPoint = 150;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool FinalCountdown = false;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("totalIntensity")]
    public float TotalIntensity = 500000f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("radius")]
    public float Radius = 500f;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("gasStorage")]
    public Dictionary<Gas, float> GasStorage = new()
    {
        {Gas.Oxygen, 0f},
        {Gas.Nitrogen, 0f},
        {Gas.CarbonDioxide, 0f},
        {Gas.Plasma, 0f},
        {Gas.Tritium, 0f},
        {Gas.WaterVapor, 0f},
        {Gas.Ammonia, 0f},
        {Gas.NitrousOxide, 0f},
        {Gas.Frezon, 0f}
    };

    /// <summary>
    /// Gas effects on supermatter based on TG station values.
    /// PowerTransmission - affects radiation/zap power output
    /// HeatModifier - affects waste heat generation (higher = more heat)
    /// HeatResistance - raises temperature damage threshold
    /// HeatPowerGeneration - affects power gain from temperature
    /// PowerlossInhibition - reduces power decay (1 = full negation)
    /// </summary>
    public readonly Dictionary<Gas, (float PowerTransmission, float HeatModifier, float HeatResistance, float HeatPowerGeneration, float PowerlossInhibition)> GasDataFields = new()
    {
        // TG station exact values
        [Gas.Oxygen] = (PowerTransmission: 0.15f, HeatModifier: 0f, HeatResistance: 0f, HeatPowerGeneration: 1f, PowerlossInhibition: 0f),
        [Gas.Nitrogen] = (PowerTransmission: 0f, HeatModifier: -2.5f, HeatResistance: 0f, HeatPowerGeneration: -1f, PowerlossInhibition: 0f),
        [Gas.CarbonDioxide] = (PowerTransmission: 0f, HeatModifier: 1f, HeatResistance: 0f, HeatPowerGeneration: 1f, PowerlossInhibition: 1f),
        [Gas.Plasma] = (PowerTransmission: 0.4f, HeatModifier: 14f, HeatResistance: 0f, HeatPowerGeneration: 1f, PowerlossInhibition: 0f),
        [Gas.Tritium] = (PowerTransmission: 3f, HeatModifier: 9f, HeatResistance: 0f, HeatPowerGeneration: 1f, PowerlossInhibition: 0f),
        [Gas.WaterVapor] = (PowerTransmission: -0.25f, HeatModifier: 11f, HeatResistance: 0f, HeatPowerGeneration: 1f, PowerlossInhibition: 0f),
        [Gas.Ammonia] = (PowerTransmission: 0f, HeatModifier: 0f, HeatResistance: 0f, HeatPowerGeneration: 0.5f, PowerlossInhibition: 0f),
        [Gas.NitrousOxide] = (PowerTransmission: 0f, HeatModifier: 0f, HeatResistance: 5f, HeatPowerGeneration: 0f, PowerlossInhibition: 0f),
        [Gas.Frezon] = (PowerTransmission: -3f, HeatModifier: -9f, HeatResistance: 0f, HeatPowerGeneration: -1f, PowerlossInhibition: 0f)
    };
}
