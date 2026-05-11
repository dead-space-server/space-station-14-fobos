using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandHierophantComponent : Component
{
    [DataField]
    public TimeSpan RangedCooldown = TimeSpan.FromSeconds(4.5);

    [DataField]
    public TimeSpan MajorAttackCooldown = TimeSpan.FromSeconds(8);

    [DataField]
    public TimeSpan ChaserCooldown = TimeSpan.FromSeconds(14);

    [DataField]
    public TimeSpan BlinkCooldown = TimeSpan.FromSeconds(7);

    [DataField]
    public TimeSpan BlinkDelay = TimeSpan.FromSeconds(0.2);

    [DataField]
    public TimeSpan CrossTelegraphDelay = TimeSpan.FromSeconds(0.2);

    [DataField]
    public TimeSpan BlastDamageDelay = TimeSpan.FromSeconds(0.6);

    [DataField]
    public TimeSpan BurstStepDelay = TimeSpan.FromSeconds(0.1);

    [DataField]
    public TimeSpan ChaserStepDelay = TimeSpan.FromSeconds(0.4);

    [DataField]
    public TimeSpan ChaserLifetime = TimeSpan.FromSeconds(6.5);

    [DataField]
    public int BaseBurstRange = 3;

    [DataField]
    public int BaseBeamRange = 5;

    [DataField]
    public int BlinkBlastRadius = 1;

    [DataField]
    public int ChaserRepathSteps = 4;

    [DataField]
    public int ChaserSwarmCount = 2;

    [DataField]
    public int MaxActiveChasers = 2;

    [DataField]
    public int MaxCrossSpamCount = 2;

    [DataField]
    public int MaxBlinkSpamCount = 2;

    [DataField]
    public int MaxBurstRange = 5;

    [DataField]
    public int MaxBeamRange = 8;

    [DataField]
    public float MajorAttackChancePerRage = 0.0025f;

    [DataField]
    public int MaxPendingBlasts = 140;

    [DataField]
    public TimeSpan PendingBlastDuplicateWindow = TimeSpan.FromSeconds(0.25);

    [DataField]
    public string TelegraphTeleportPrototype = "LavalandHierophantTelegraphTeleport";

    [DataField]
    public string SquaresPrototype = "LavalandHierophantSquares";

    [DataField]
    public DamageSpecifier BlastDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", FixedPoint2.New(18) },
            { "Blunt", FixedPoint2.New(8) },
        },
    };

    [DataField]
    public DamageSpecifier BlinkDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", FixedPoint2.New(30) },
            { "Blunt", FixedPoint2.New(10) },
        },
    };

    [DataField]
    public string TelegraphPrototype = "LavalandHierophantTelegraph";

    [DataField]
    public string TelegraphCardinalPrototype = "LavalandHierophantTelegraphCardinal";

    [DataField]
    public string TelegraphDiagonalPrototype = "LavalandHierophantTelegraphDiagonal";

    [DataField]
    public string BlastPrototype = "LavalandHierophantBlast";

    [DataField]
    public SoundSpecifier CrossTelegraphSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/bin_close.ogg");

    [DataField]
    public SoundSpecifier BlinkDestinationSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/wand_teleport.ogg");

    [DataField]
    public SoundSpecifier BlinkSourceSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/airlockopen.ogg");

    [DataField]
    public SoundSpecifier BurstSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/airlockopen.ogg");

    [DataField]
    public SoundSpecifier BlastSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/blind.ogg");

    [DataField]
    public SoundSpecifier HitSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/sear.ogg");

    [DataField]
    public SoundSpecifier MovementTrailSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/mechmove04.ogg");

    [DataField]
    public TimeSpan MovementTrailInterval = TimeSpan.FromSeconds(1.1);

    [ViewVariables]
    public TimeSpan NextAttack;

    [ViewVariables]
    public TimeSpan NextChaser;

    [ViewVariables]
    public TimeSpan NextBlink;

    [ViewVariables]
    public TimeSpan BusyUntil;

    [ViewVariables]
    public TimeSpan NextMovementTrail;

    [ViewVariables]
    public readonly List<LavalandHierophantPendingBlast> PendingBlasts = new();

    [ViewVariables]
    public readonly List<LavalandHierophantPendingTeleport> PendingTeleports = new();

    [ViewVariables]
    public readonly List<LavalandHierophantChaser> Chasers = new();
}

public sealed class LavalandHierophantPendingBlast
{
    public EntityUid Grid;
    public Vector2i Tile;
    public TimeSpan DetonateAt;
    public TimeSpan TelegraphAt;
    public string? TelegraphPrototype;
    public bool Telegraphed;
    public string BlastPrototype = string.Empty;
    public DamageSpecifier Damage = new();
}

public sealed class LavalandHierophantPendingTeleport
{
    public EntityUid Boss;
    public EntityUid Grid;
    public Vector2i Destination;
    public TimeSpan ExecuteAt;
}

public sealed class LavalandHierophantChaser
{
    public EntityUid Grid;
    public EntityUid Target;
    public Vector2i Tile;
    public Vector2i MovingDirection;
    public Vector2i PreviousDirection;
    public Vector2i OlderDirection;
    public int StepsBeforeRepath;
    public TimeSpan NextStep;
    public TimeSpan ExpiresAt;
    public TimeSpan StepDelay;
}
