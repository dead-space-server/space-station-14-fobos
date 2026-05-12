using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandBossArenaComponent : Component
{
    [ViewVariables]
    public int ArenaId;

    [ViewVariables]
    public EntityUid Grid = EntityUid.Invalid;

    [ViewVariables]
    public EntityUid Map = EntityUid.Invalid;

    [ViewVariables]
    public EntityUid Boss = EntityUid.Invalid;

    [ViewVariables]
    public Vector2i BossSpawnTile = Vector2i.Zero;

    [ViewVariables]
    public string BossName = "boss";

    [ViewVariables]
    public int Width = 35;

    [ViewVariables]
    public int Height = 35;

    [ViewVariables]
    public float MaxHealth = 1000f;

    [ViewVariables]
    public float FightStartDistance = 8f;

    [ViewVariables]
    public readonly HashSet<NetUserId> Participants = new();

    [ViewVariables]
    public readonly Dictionary<NetUserId, MapCoordinates> ReturnCoordinates = new();

    [ViewVariables]
    public TimeSpan NextParticipantScan;

    [ViewVariables]
    public TimeSpan NextHudUpdate;

    [ViewVariables]
    public TimeSpan NextBossLeashCheck;

    [ViewVariables]
    public TimeSpan? BossOutsideArenaSince;

    [ViewVariables]
    public TimeSpan? EmptySince;

    [ViewVariables]
    public TimeSpan? CleanupAt;

    [ViewVariables]
    public bool ResetOnEmpty = true;

    [ViewVariables]
    public TimeSpan EmptyResetDelay = TimeSpan.FromSeconds(20);

    [ViewVariables]
    public float EmptyResetHealFraction = 1f;

    [ViewVariables]
    public float EmptyResetMinHeal;

    [ViewVariables]
    public bool HasResetWhileEmpty;

    [ViewVariables]
    public bool DeleteOnEmpty;

    [ViewVariables]
    public bool DeleteOnBossDeath;

    [ViewVariables]
    public bool ReturnParticipantsOnDelete;

    [ViewVariables]
    public bool FightStarted;

    [ViewVariables]
    public bool Ended;
}
