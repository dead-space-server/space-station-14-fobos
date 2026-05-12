using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandAshDrakeComponent : Component
{
    [DataField]
    public TimeSpan RangedCooldown = TimeSpan.FromSeconds(3.2);

    [DataField]
    public TimeSpan ForcePressureAfter = TimeSpan.FromSeconds(6);

    [DataField]
    public TimeSpan TargetSwitchCooldown = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan TargetPressureMemory = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan FireWallStepDelay = TimeSpan.FromSeconds(0.06);

    [DataField]
    public TimeSpan FireRainDelay = TimeSpan.FromSeconds(0.9);

    [DataField]
    public TimeSpan SwoopWindup = TimeSpan.FromSeconds(0.6);

    [DataField]
    public TimeSpan SwoopStepDelay = TimeSpan.FromSeconds(0.085);

    [DataField]
    public TimeSpan SwoopRecover = TimeSpan.FromSeconds(0.35);

    [DataField]
    public TimeSpan ChainedSwoopDelay = TimeSpan.FromSeconds(0.28);

    [DataField]
    public int FireWallRange = 10;

    [DataField]
    public int FireRainRadius = 9;

    [DataField]
    public float FireRainTileChance = 0.14f;

    [DataField]
    public int FireRainMaxTiles = 24;

    [DataField]
    public int SwoopSteps = 36;

    [DataField]
    public int TripleSwoopSteps = 28;

    [DataField]
    public int SwoopFireRainMaxTiles = 12;

    [DataField]
    public int MaxPendingTiles = 160;

    [DataField]
    public float FireStacks = 2.5f;

    [DataField]
    public float SwoopThrowSpeed = 7.5f;

    [DataField]
    public string FirePrototype = "LavalandAshDrakeFire";

    [DataField]
    public string FireRainTargetPrototype = "LavalandAshDrakeTarget";

    [DataField]
    public string FireRainFireballPrototype = "LavalandAshDrakeFireball";

    [DataField]
    public string LandingPrototype = "LavalandAshDrakeLanding";

    [DataField]
    public DamageSpecifier FireWallDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", FixedPoint2.New(20) },
        },
    };

    [DataField]
    public DamageSpecifier FireRainDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", FixedPoint2.New(40) },
        },
    };

    [DataField]
    public DamageSpecifier SwoopDamage = new()
    {
        DamageDict = new()
        {
            { "Blunt", FixedPoint2.New(45) },
            { "Heat", FixedPoint2.New(30) },
        },
    };

    [DataField]
    public SoundSpecifier FireSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/AshDrake/fireball.ogg");

    [DataField]
    public SoundSpecifier FireRainSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/AshDrake/fleshtostone.ogg");

    [DataField]
    public SoundSpecifier ImpactSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/AshDrake/meteorimpact.ogg");

    [DataField]
    public SoundSpecifier HitSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/AshDrake/sear.ogg");

    [ViewVariables]
    public TimeSpan NextAttack;

    [ViewVariables]
    public TimeSpan BusyUntil;

    [ViewVariables]
    public TimeSpan LastPressureAt;

    [ViewVariables]
    public string LastAttackKind = string.Empty;

    [ViewVariables]
    public EntityUid? CurrentPrimaryTarget;

    [ViewVariables]
    public TimeSpan LastTargetSwitchAt;

    [ViewVariables]
    public readonly Dictionary<EntityUid, TimeSpan> LastPressureByTarget = new();

    [ViewVariables]
    public bool Swooping;

    [ViewVariables]
    public bool SwoopInvulnerable;

    [ViewVariables]
    public EntityUid? SwoopTarget;

    [ViewVariables]
    public int SwoopRemainingSteps;

    [ViewVariables]
    public bool SwoopDropsFireRain;

    [ViewVariables]
    public int SwoopFireRainTilesQueued;

    [ViewVariables]
    public TimeSpan NextSwoopStep;

    [ViewVariables]
    public TimeSpan SwoopImpactAt;

    [ViewVariables]
    public int PendingSwoops;

    [ViewVariables]
    public int PendingSwoopSteps;

    [ViewVariables]
    public TimeSpan NextQueuedSwoop;

    [ViewVariables]
    public readonly List<LavalandAshDrakePendingTile> PendingTiles = new();
}

public sealed class LavalandAshDrakePendingTile
{
    public EntityUid Grid;
    public Vector2i Tile;
    public TimeSpan DetonateAt;
    public DamageSpecifier Damage = new();
    public bool Ignite;
    public bool PlayImpactSound;
    public string EffectPrototype = string.Empty;
}
