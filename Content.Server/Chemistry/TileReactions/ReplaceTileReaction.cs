using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Chemistry.TileReactions;

/// <summary>
/// Replaces specific tiles when a reagent reacts with them.
/// </summary>
[DataDefinition]
public sealed partial class ReplaceTileReaction : ITileReaction
{
    [DataField(required: true)]
    public Dictionary<string, string> Replacements = new();

    [DataField]
    public FixedPoint2 Cost = FixedPoint2.New(0.25f);

    public FixedPoint2 TileReact(
        TileRef tile,
        ReagentPrototype reagent,
        FixedPoint2 reactVolume,
        IEntityManager entityManager,
        List<ReagentData>? data)
    {
        if (reactVolume < Cost ||
            tile.Tile.IsEmpty ||
            !entityManager.TryGetComponent<MapGridComponent>(tile.GridUid, out var grid))
        {
            return FixedPoint2.Zero;
        }

        var tileDefs = IoCManager.Resolve<ITileDefinitionManager>();
        if (!tileDefs.TryGetDefinition(tile.Tile.TypeId, out var current) ||
            !Replacements.TryGetValue(current.ID, out var replacementId) ||
            !tileDefs.TryGetDefinition(replacementId, out var replacement))
        {
            return FixedPoint2.Zero;
        }

        var map = entityManager.System<SharedMapSystem>();
        var variant = tile.Tile.Variant < replacement.Variants ? tile.Tile.Variant : (byte) 0;
        map.SetTile(tile.GridUid, grid, tile.GridIndices, new Tile(replacement.TileId, tile.Tile.Flags, variant, tile.Tile.RotationMirroring));

        return Cost;
    }
}
