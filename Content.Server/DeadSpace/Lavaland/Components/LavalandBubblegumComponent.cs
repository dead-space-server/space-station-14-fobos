using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandBubblegumComponent : Component
{
    [DataField]
    public TimeSpan RangedCooldown = TimeSpan.FromSeconds(3.4);

    [DataField]
    public TimeSpan ForcePressureAfter = TimeSpan.FromSeconds(7);

    [DataField]
    public TimeSpan TargetSwitchCooldown = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan TargetPressureMemory = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan BloodSmackDelay = TimeSpan.FromSeconds(0.35);

    [DataField]
    public TimeSpan BloodGrabDelay = TimeSpan.FromSeconds(0.65);

    [DataField]
    public TimeSpan BloodWarpDelay = TimeSpan.FromSeconds(0.35);

    [DataField]
    public TimeSpan BloodHandRecover = TimeSpan.FromSeconds(0.45);

    [DataField]
    public TimeSpan BloodReactionCooldown = TimeSpan.FromSeconds(2.25);

    [DataField]
    public TimeSpan BloodSprayStepDelay = TimeSpan.FromSeconds(0.055);

    [DataField]
    public TimeSpan ChargeWindup = TimeSpan.FromSeconds(0.45);

    [DataField]
    public TimeSpan ChargeStepDelay = TimeSpan.FromSeconds(0.05);

    [DataField]
    public TimeSpan ChargeRecover = TimeSpan.FromSeconds(0.4);

    [DataField]
    public TimeSpan ChainedChargeDelay = TimeSpan.FromSeconds(0.3);

    [DataField]
    public TimeSpan SummonCooldown = TimeSpan.FromSeconds(10);

    [DataField]
    public int BloodSprayBaseRange = 8;

    [DataField]
    public float BloodSprayRageRangeMultiplier = 0.4f;

    [DataField]
    public int MaxBloodPools = 90;

    [DataField]
    public int MaxPendingBloodTiles = 80;

    [DataField]
    public int ChargeMaxSteps = 36;

    [DataField]
    public int TripleChargeSteps = 28;

    [DataField]
    public int MaxActiveSlaughterlings = 6;

    [DataField]
    public int MaxSummonsPerCast = 6;

    [DataField]
    public float ChargeThrowSpeed = 7f;

    [DataField]
    public float BloodGrabChance = 0.25f;

    [DataField]
    public float BloodGrabChanceBelowHalf = 0.4f;

    [DataField]
    public string BloodPoolPrototype = "LavalandBubblegumBloodPool";

    [DataField]
    public string BloodSplatterPrototype = "LavalandBubblegumBloodSplatter";

    [DataField]
    public string BloodGibsPrototype = "LavalandBubblegumBloodGibs";

    [DataField]
    public string LandingPrototype = "LavalandBubblegumLanding";

    [DataField]
    public string RightSmackPrototype = "LavalandBubblegumRightSmack";

    [DataField]
    public string LeftSmackPrototype = "LavalandBubblegumLeftSmack";

    [DataField]
    public string RightPawPrototype = "LavalandBubblegumRightPaw";

    [DataField]
    public string LeftPawPrototype = "LavalandBubblegumLeftPaw";

    [DataField]
    public string RightThumbPrototype = "LavalandBubblegumRightThumb";

    [DataField]
    public string LeftThumbPrototype = "LavalandBubblegumLeftThumb";

    [DataField]
    public string SlaughterlingPrototype = "MobLavalandBubblegumSlaughterling";

    [DataField]
    public DamageSpecifier SmackDamage = new()
    {
        DamageDict = new()
        {
            { "Slash", FixedPoint2.New(25) },
        },
    };

    [DataField]
    public DamageSpecifier GrabDamage = new()
    {
        DamageDict = new()
        {
            { "Slash", FixedPoint2.New(15) },
        },
    };

    [DataField]
    public DamageSpecifier ChargeDamage = new()
    {
        DamageDict = new()
        {
            { "Blunt", FixedPoint2.New(40) },
        },
    };

    [DataField]
    public SoundSpecifier AttackSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Bubblegum/demon_attack1.ogg");

    [DataField]
    public SoundSpecifier EnterBloodSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Bubblegum/enter_blood.ogg");

    [DataField]
    public SoundSpecifier ExitBloodSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Bubblegum/exit_blood.ogg");

    [DataField]
    public SoundSpecifier ImpactSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Bubblegum/meteorimpact.ogg");

    [DataField]
    public SoundSpecifier SplatSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Bubblegum/splat.ogg");

    [ViewVariables]
    public TimeSpan NextAttack;

    [ViewVariables]
    public TimeSpan BusyUntil;

    [ViewVariables]
    public TimeSpan NextSummon;

    [ViewVariables]
    public TimeSpan NextBloodReaction;

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
    public bool Charging;

    [ViewVariables]
    public Vector2i ChargeTargetTile;

    [ViewVariables]
    public int ChargeRemainingSteps;

    [ViewVariables]
    public TimeSpan NextChargeStep;

    [ViewVariables]
    public int PendingCharges;

    [ViewVariables]
    public int PendingChargeSteps;

    [ViewVariables]
    public TimeSpan NextQueuedCharge;

    [ViewVariables]
    public readonly HashSet<EntityUid> ChargeHitEntities = new();

    [ViewVariables]
    public readonly List<EntityUid> BloodPools = new();

    [ViewVariables]
    public readonly List<EntityUid> Slaughterlings = new();

    [ViewVariables]
    public readonly List<LavalandBubblegumPendingBloodTile> PendingBloodTiles = new();

    [ViewVariables]
    public readonly List<LavalandBubblegumPendingHandAttack> PendingHandAttacks = new();
}

[RegisterComponent]
public sealed partial class LavalandBubblegumBloodPoolComponent : Component
{
    [ViewVariables]
    public EntityUid Grid;

    [ViewVariables]
    public Vector2i Tile;
}

public sealed class LavalandBubblegumPendingBloodTile
{
    public EntityUid Grid;
    public Vector2i Tile;
    public TimeSpan SpawnAt;
}

public sealed class LavalandBubblegumPendingHandAttack
{
    public EntityUid Grid;
    public Vector2i Tile;
    public TimeSpan AttackAt;
    public bool Grab;
    public bool RightHand;
}
