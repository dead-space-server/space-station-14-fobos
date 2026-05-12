using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Lavaland.Bosses;

[Prototype]
public sealed partial class LavalandBossArenaPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public EntProtoId BossPrototype = default;

    [DataField]
    public string ArenaName = "Lavaland Boss Arena";

    [DataField]
    public string FloorTile = "FloorBasalt";

    [DataField]
    public EntProtoId? FloorVisualPrototype;

    [DataField]
    public EntProtoId? WallPrototype;

    [DataField]
    public EntProtoId? LightPrototype;

    [DataField]
    public int Size = 35;

    [DataField]
    public float FightStartDistance;

    [DataField]
    public int SpawnAttempts = 128;

    [DataField]
    public float MinDistance = 170f;

    [DataField]
    public float MaxDistance = 320f;

    [DataField]
    public float MapEdgePadding = 32f;

    [DataField]
    public float LandingExclusionRadius = 90f;

    [DataField]
    public float OutpostExclusionRadius = 100f;

    [DataField]
    public float FtlBeaconExclusionRadius = 64f;

    [DataField]
    public float GridSeparation = 32f;

    [DataField]
    public bool DeleteOnEmpty;

    [DataField]
    public bool DeleteOnBossDeath;

    [DataField]
    public bool ReturnParticipantsOnDelete;

    [DataField]
    public bool ResetOnEmpty = true;

    [DataField]
    public TimeSpan EmptyResetDelay = TimeSpan.FromSeconds(20);

    [DataField]
    public float EmptyResetHealFraction = 1f;

    [DataField]
    public float EmptyResetMinHeal;
}
