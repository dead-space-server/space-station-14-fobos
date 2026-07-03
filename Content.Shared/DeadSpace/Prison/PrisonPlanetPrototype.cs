using System.Numerics;
using Content.Shared.Atmos;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Parallax.Biomes.Markers;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.Prison;

[Prototype]
public sealed partial class PrisonPlanetPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ProtoId<BiomeTemplatePrototype> Biome = default!;

    [DataField]
    public Color? LightColor = Color.FromHex("#8DA8C6");

    [DataField]
    public GasMixture? Atmosphere;

    [DataField]
    public bool Gravity = true;

    [DataField]
    public string MapName = "Prison Quarry";

    [DataField]
    public int MapHalfSize = 250;

    [DataField]
    public bool BoundaryEnabled = true;

    [DataField]
    public int BoundaryWallWidth = 6;

    [DataField]
    public string BoundaryTile = "FloorChromite";

    [DataField]
    public string BoundaryWallEntity = "WallRockChromitePrisonBoundary";

    [DataField]
    public bool ResidenceReservationEnabled = true;

    [DataField]
    public int ResidenceReservationSize = 112;

    [DataField]
    public string ResidenceTile = "FloorSnowDug";

    [DataField]
    public ResPath? ResidenceGridPath;

    [DataField]
    public Vector2 ResidenceGridOffset = Vector2.Zero;

    [DataField]
    public string? ResidenceGridName = "Prison Base";

    [DataField]
    public List<ProtoId<BiomeMarkerLayerPrototype>> MarkerLayers = new();
}
