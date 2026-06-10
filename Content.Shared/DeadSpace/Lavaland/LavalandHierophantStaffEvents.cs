using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Shared.DeadSpace.Lavaland;

public abstract partial class LavalandHierophantStaffActionEvent : WorldTargetActionEvent
{
    [DataField]
    public string TelegraphPrototype = "LavalandHierophantTelegraph";

    [DataField]
    public string SquaresPrototype = "LavalandHierophantSquares";

    [DataField]
    public string BlastPrototype = "LavalandHierophantBlast";

    [DataField]
    public SoundSpecifier TelegraphSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/bin_close.ogg");

    [DataField]
    public SoundSpecifier BlastSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/blind.ogg");

    [DataField]
    public SoundSpecifier HitSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/sear.ogg");

    [DataField]
    public TimeSpan TelegraphDelay = TimeSpan.FromSeconds(0.55);

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Heat", FixedPoint2.New(20) },
            { "Blunt", FixedPoint2.New(8) },
        },
    };
}

public sealed partial class LavalandHierophantStaffCrossActionEvent : LavalandHierophantStaffActionEvent
{
    [DataField]
    public int BeamRange = 5;
}

public sealed partial class LavalandHierophantStaffBurstActionEvent : LavalandHierophantStaffActionEvent
{
    [DataField]
    public int Radius = 3;

    [DataField]
    public TimeSpan StepDelay = TimeSpan.FromSeconds(0.08);
}

public sealed partial class LavalandHierophantStaffBlinkActionEvent : LavalandHierophantStaffActionEvent
{
    [DataField]
    public string TeleportTelegraphPrototype = "LavalandHierophantTelegraphTeleport";

    [DataField]
    public SoundSpecifier BlinkSourceSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/airlockopen.ogg");

    [DataField]
    public SoundSpecifier BlinkDestinationSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/wand_teleport.ogg");

    [DataField]
    public TimeSpan BlinkDelay = TimeSpan.FromSeconds(0.2);

    [DataField]
    public int BlastRadius = 1;

    [DataField]
    public DamageSpecifier BlinkDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", FixedPoint2.New(16) },
            { "Blunt", FixedPoint2.New(6) },
        },
    };
}

public sealed partial class LavalandHierophantStaffChaserActionEvent : EntityTargetActionEvent
{
    [DataField]
    public string SquaresPrototype = "LavalandHierophantSquares";

    [DataField]
    public string BlastPrototype = "LavalandHierophantBlast";

    [DataField]
    public SoundSpecifier TelegraphSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/bin_close.ogg");

    [DataField]
    public SoundSpecifier BlastSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/blind.ogg");

    [DataField]
    public SoundSpecifier HitSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/Hierophant/sear.ogg");

    [DataField]
    public TimeSpan TelegraphDelay = TimeSpan.FromSeconds(0.45);

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Heat", FixedPoint2.New(14) },
            { "Blunt", FixedPoint2.New(6) },
        },
    };

    [DataField]
    public int MaxSteps = 7;

    [DataField]
    public TimeSpan StepDelay = TimeSpan.FromSeconds(0.16);
}
