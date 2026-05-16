using System.Numerics;
using Content.Shared.Atmos;
using Content.Shared.DeadSpace.Lavaland.Bosses;
using Content.Shared.Damage;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Parallax.Biomes.Markers;
using Content.Shared.Procedural;
using Content.Shared.Weather;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.Lavaland;

[Prototype]
public sealed partial class LavalandPlanetPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ProtoId<BiomeTemplatePrototype> Biome = default!;

    [DataField]
    public Color? LightColor = Color.FromHex("#A34931");

    [DataField]
    public GasMixture? Atmosphere;

    [DataField]
    public bool Gravity = true;

    [DataField]
    public string MapName = "Lavaland";

    [DataField]
    public int MapHalfSize = 350;

    [DataField]
    public bool BoundaryEnabled = true;

    [DataField]
    public int BoundaryWallWidth = 2;

    [DataField]
    public int BoundaryLavaWidth = 3;

    [DataField]
    public string BoundaryTile = "FloorBasalt";

    [DataField]
    public string BoundaryWallEntity = "WallRockBasaltLavalandBoundary";

    [DataField]
    public string BoundaryLavaEntity = "FloorLavaEntity";

    [DataField]
    public int LandingPadRadius = 24;

    [DataField]
    public string LandingPadTile = "FloorBasalt";

    [DataField]
    public bool TerminalReservationEnabled = true;

    [DataField]
    public int TerminalReservationSize = 70;

    [DataField]
    public string TerminalTile = "FloorBasalt";

    [DataField]
    public ResPath? TerminalGridPath;

    [DataField]
    public Vector2 TerminalGridOffset = Vector2.Zero;

    [DataField]
    public string? TerminalGridName = "Lavaland Mining Outpost";

    [DataField]
    public int MinStructureDistance = 80;

    [DataField]
    public int MaxStructureDistance = 170;

    [DataField]
    public int MinStructureSeparation = 40;

    [DataField]
    public Dictionary<ProtoId<DungeonConfigPrototype>, int> Structures = new();

    [DataField]
    public ProtoId<DungeonConfigPrototype>? LandingSite;

    [DataField]
    public bool TendrilsEnabled = true;

    [DataField]
    public int TendrilSpawnCount = 6;

    [DataField]
    public TimeSpan TendrilRespawnInterval = TimeSpan.FromMinutes(20);

    [DataField]
    public int TendrilRespawnBatchMin = 1;

    [DataField]
    public int TendrilRespawnBatchMax = 1;

    [DataField]
    public int TendrilSpawnAttempts = 256;

    [DataField]
    public int TendrilMinDistance = 120;

    [DataField]
    public int TendrilMaxDistance = 320;

    [DataField]
    public int TendrilMinSeparation = 80;

    [DataField]
    public int TendrilClearRadius = 1;

    [DataField]
    public float TendrilMapEdgePadding = 24f;

    [DataField]
    public float TendrilOutpostExclusionRadius = 100f;

    [DataField]
    public float TendrilLandingExclusionRadius = 56f;

    [DataField]
    public float TendrilFtlBeaconExclusionRadius = 56f;

    [DataField]
    public List<LavalandTendrilSpawnEntry> TendrilSpawns = new()
    {
        new()
        {
            Prototype = "LavalandNecropolisTendrilWatcher",
            Weight = 3,
            MaxCount = 3,
        },
        new()
        {
            Prototype = "LavalandNecropolisTendrilGoliath",
            Weight = 2,
            MaxCount = 2,
        },
        new()
        {
            Prototype = "LavalandNecropolisTendrilLegion",
            Weight = 1,
            MaxCount = 1,
        },
    };

    [DataField]
    public Dictionary<ProtoId<LavalandBossArenaPrototype>, int> BossArenas = new();

    [DataField]
    public List<ProtoId<BiomeMarkerLayerPrototype>> MarkerLayers = new()
    {
        "LavalandOreIron",
        "LavalandOreCoal",
        "LavalandOrePlasma",
        "LavalandOreQuartz",
        "LavalandOreSalt",
        "LavalandOreSilver",
        "LavalandOreGold",
        "LavalandOreMagmite",
        "LavalandOreUranium",
        "LavalandOreBananium",
        "LavalandOreArtifactFragment",
        "LavalandOreDiamond",
        "LavalandOreBluespaceCrystal",
        "LavalandChasms",
        "LavalandChasmsMedium",
        "LavalandChasmsLarge",
    };

    [DataField]
    public bool FaunaEnabled = true;

    [DataField]
    public int FaunaInitialSpawnCount = 38;

    [DataField]
    public int FaunaSoftCap = 45;

    [DataField]
    public int FaunaHardCap = 60;

    [DataField]
    public TimeSpan FaunaUpdateInterval = TimeSpan.FromSeconds(90);

    [DataField]
    public int FaunaSpawnBatchMin = 1;

    [DataField]
    public int FaunaSpawnBatchMax = 2;

    [DataField]
    public int FaunaLowPopulationThreshold = 20;

    [DataField]
    public int FaunaLowPopulationBatchMax = 4;

    [DataField]
    public int FaunaSpawnAttempts = 64;

    [DataField]
    public int FaunaSpawnClearance = 1;

    [DataField]
    public float FaunaMapEdgePadding = 18f;

    [DataField]
    public float FaunaMinPlayerDistance = 32f;

    [DataField]
    public float FaunaOutpostExclusionRadius = 80f;

    [DataField]
    public float FaunaLandingExclusionRadius = 36f;

    [DataField]
    public float FaunaFtlBeaconExclusionRadius = 32f;

    [DataField]
    public int FaunaSectorSize = 48;

    [DataField]
    public TimeSpan FaunaSectorCooldown = TimeSpan.FromMinutes(8);

    [DataField]
    public List<LavalandFaunaSpawnEntry> FaunaSpawns = new();

    [DataField]
    public bool FtlEnabled = true;

    [DataField]
    public bool FtlBeaconsOnly = true;

    [DataField]
    public bool RequireCoordinateDisk;

    [DataField]
    public EntityWhitelist? FtlWhitelist;

    [DataField]
    public EntityWhitelist? FtlDockWhitelist;

    [DataField]
    public string FtlBeaconName = "Lavaland Landing Zone";

    [DataField]
    public Vector2 FtlBeaconOffset = new(0f, -180f);

    [DataField]
    public float FtlFallbackMinOffset = 8f;

    [DataField]
    public float FtlFallbackMaxOffset = 96f;

    [DataField]
    public bool AshStormEnabled = true;

    [DataField]
    public ProtoId<WeatherPrototype>? AshStormWarningWeather = "AshfallLight";

    [DataField]
    public ProtoId<WeatherPrototype>? AshStormWeather = "AshfallHeavy";

    [DataField]
    public TimeSpan AshStormWarningDuration = TimeSpan.FromSeconds(25);

    [DataField]
    public TimeSpan AshStormDurationMin = TimeSpan.FromSeconds(50);

    [DataField]
    public TimeSpan AshStormDurationMax = TimeSpan.FromSeconds(100);

    [DataField]
    public TimeSpan AshStormCooldownMin = TimeSpan.FromSeconds(300);

    [DataField]
    public TimeSpan AshStormCooldownMax = TimeSpan.FromSeconds(600);

    [DataField]
    public TimeSpan AshStormDamageInterval = TimeSpan.FromSeconds(1);

    [DataField]
    public DamageSpecifier AshStormDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", 4.0f },
        },
    };
}

[DataDefinition]
public sealed partial class LavalandFaunaSpawnEntry
{
    [DataField(required: true)]
    public EntProtoId Prototype = default;

    [DataField]
    public int Weight = 1;

    [DataField]
    public int MaxCount = 8;
}

[DataDefinition]
public sealed partial class LavalandTendrilSpawnEntry
{
    [DataField(required: true)]
    public EntProtoId Prototype = default;

    [DataField]
    public int Weight = 1;

    [DataField]
    public int MaxCount = 8;
}
