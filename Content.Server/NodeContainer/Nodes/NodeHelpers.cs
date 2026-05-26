using System.Diagnostics.CodeAnalysis;
using Content.Shared.NodeContainer;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.NodeContainer.Nodes
{
    /// <summary>
    ///     Helper utilities for implementing <see cref="Node"/>.
    /// </summary>
    public static class NodeHelpers
    {
        public static IEnumerable<Node> GetNodesInTile(EntityQuery<NodeContainerComponent> nodeQuery, SharedMapSystem map, EntityUid gridUid, MapGridComponent grid, Vector2i coords)
        {
            foreach (var entityUid in map.GetAnchoredEntities(gridUid, grid, coords))
            {
                if (!nodeQuery.TryGetComponent(entityUid, out var container))
                    continue;

                foreach (var node in container.Nodes.Values)
                {
                    yield return node;
                }
            }
        }

        public static IEnumerable<(Direction dir, Node node)> GetCardinalNeighborNodes(
            EntityQuery<NodeContainerComponent> nodeQuery,
            SharedMapSystem map,
            EntityUid gridUid,
            MapGridComponent grid,
            Vector2i coords,
            bool includeSameTile = true)
        {
            foreach (var (dir, entityUid) in GetCardinalNeighborCells(map, gridUid, grid, coords, includeSameTile))
            {
                if (!nodeQuery.TryGetComponent(entityUid, out var container))
                    continue;

                foreach (var node in container.Nodes.Values)
                {
                    yield return (dir, node);
                }
            }
        }

        [SuppressMessage("ReSharper", "EnforceForeachStatementBraces")]
        public static IEnumerable<(Direction dir, EntityUid entity)> GetCardinalNeighborCells(
            SharedMapSystem map,
            EntityUid gridUid,
            MapGridComponent grid,
            Vector2i coords,
            bool includeSameTile = true)
        {
            if (includeSameTile)
            {
                foreach (var uid in map.GetAnchoredEntities(gridUid, grid, coords))
                    yield return (Direction.Invalid, uid);
            }

            foreach (var uid in map.GetAnchoredEntities(gridUid, grid, coords + (0, 1)))
                yield return (Direction.North, uid);

            foreach (var uid in map.GetAnchoredEntities(gridUid, grid, coords + (0, -1)))
                yield return (Direction.South, uid);

            foreach (var uid in map.GetAnchoredEntities(gridUid, grid, coords + (1, 0)))
                yield return (Direction.East, uid);

            foreach (var uid in map.GetAnchoredEntities(gridUid, grid, coords + (-1, 0)))
                yield return (Direction.West, uid);
        }
    }
}
