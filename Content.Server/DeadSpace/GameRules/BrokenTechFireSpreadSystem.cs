// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System;
using System.Collections.Generic;
using Content.Server.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DeadSpace.GameRules.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Item;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.GameRules;

public sealed class BrokenTechFireSpreadSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    private readonly List<SpreadRequest> _spreadQueue = new();
    private readonly HashSet<EntityUid> _tileEntities = new();

    private static readonly AtmosDirection[] Directions =
    {
        AtmosDirection.North,
        AtmosDirection.East,
        AtmosDirection.South,
        AtmosDirection.West,
    };

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _spreadQueue.Clear();

        var query = EntityQueryEnumerator<BrokenTechFireSpreadComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var fire, out var xform))
        {
            if (TerminatingOrDeleted(uid))
                continue;

            if (xform.GridUid is not { } gridUid ||
                !TryComp<MapGridComponent>(gridUid, out var grid))
            {
                fire.Finished = true;
                continue;
            }

            var tile = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);
            InitializeOrigin(fire, gridUid, tile);

            if (IsWaterTile(gridUid, grid, tile, fire))
            {
                QueueDel(uid);
                continue;
            }

            if (fire.Finished)
                continue;

            if (fire.Distance >= fire.MaxRadius)
            {
                fire.Finished = true;
                continue;
            }

            if (_timing.CurTime < fire.NextSpread)
                continue;

            fire.NextSpread = _timing.CurTime + TimeSpan.FromSeconds(fire.SpreadDelay);

            _spreadQueue.Add(new SpreadRequest(uid, fire, gridUid, grid, tile));
        }

        foreach (var request in _spreadQueue)
        {
            if (TerminatingOrDeleted(request.Uid) || request.Fire.Finished)
                continue;

            if (!TrySpread(request.Uid, request.Fire, request.GridUid, request.Grid, request.Tile))
                request.Fire.Finished = true;
        }

        _spreadQueue.Clear();
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BrokenTechFireSpreadComponent, ReactionEntityEvent>(OnReaction);
    }

    private void OnReaction(Entity<BrokenTechFireSpreadComponent> ent, ref ReactionEntityEvent args)
    {
        if (args.Method != ReactionMethod.Touch)
            return;

        if (!HasWaterReagent(args.Reagent.ID, ent.Comp))
            return;

        QueueDel(ent);
    }

    private void InitializeOrigin(BrokenTechFireSpreadComponent fire, EntityUid gridUid, Vector2i tile)
    {
        if (fire.HasOrigin)
            return;

        fire.OriginGrid = gridUid;
        fire.OriginTile = tile;
        fire.HasOrigin = true;
        fire.Distance = 0;
        fire.NextSpread = _timing.CurTime + TimeSpan.FromSeconds(fire.SpreadDelay);
    }

    private bool TrySpread(
        EntityUid uid,
        BrokenTechFireSpreadComponent fire,
        EntityUid gridUid,
        MapGridComponent grid,
        Vector2i tile)
    {
        var prototype = MetaData(uid).EntityPrototype?.ID;
        if (prototype == null || fire.OriginGrid != gridUid)
            return false;

        var spawned = false;
        foreach (var direction in Directions)
        {
            var neighborTile = tile.Offset(direction);
            var opposite = direction.GetOpposite();

            if (!_map.TryGetTileRef(gridUid, grid, neighborTile, out var tileRef) || tileRef.Tile.IsEmpty)
                continue;

            if (IsBlockedByAirtight(gridUid, grid, tile, direction) ||
                IsBlockedByAirtight(gridUid, grid, neighborTile, opposite) ||
                IsWaterTile(gridUid, grid, neighborTile, fire) ||
                HasBrokenTechFireAt(gridUid, grid, neighborTile))
            {
                continue;
            }

            var child = Spawn(prototype, _map.GridTileToLocal(gridUid, grid, neighborTile));
            if (!TryComp<BrokenTechFireSpreadComponent>(child, out var childFire))
                continue;

            childFire.OriginGrid = fire.OriginGrid;
            childFire.OriginTile = fire.OriginTile;
            childFire.HasOrigin = true;
            childFire.Distance = fire.Distance + 1;
            childFire.MaxRadius = fire.MaxRadius;
            childFire.SpreadDelay = fire.SpreadDelay;
            childFire.WaterReagents = new(fire.WaterReagents);
            childFire.NextSpread = _timing.CurTime + TimeSpan.FromSeconds(childFire.SpreadDelay);
            spawned = true;
        }

        return spawned;
    }

    private bool IsBlockedByAirtight(EntityUid gridUid, MapGridComponent grid, Vector2i tile, AtmosDirection direction)
    {
        var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, tile);
        var airtightQuery = GetEntityQuery<AirtightComponent>();

        while (anchored.MoveNext(out var ent))
        {
            if (!airtightQuery.TryGetComponent(ent, out var airtight) || !airtight.AirBlocked)
                continue;

            if ((airtight.AirBlockedDirection & direction) != 0x0)
                return true;
        }

        return false;
    }

    private bool IsWaterTile(
        EntityUid gridUid,
        MapGridComponent grid,
        Vector2i tile,
        BrokenTechFireSpreadComponent fire)
    {
        var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, tile);
        var puddleQuery = GetEntityQuery<PuddleComponent>();
        var solutionQuery = GetEntityQuery<SolutionContainerManagerComponent>();
        var itemQuery = GetEntityQuery<ItemComponent>();
        var xformQuery = GetEntityQuery<TransformComponent>();

        while (anchored.MoveNext(out var ent))
        {
            if (!puddleQuery.TryGetComponent(ent, out var puddle) ||
                !_solutionContainer.ResolveSolution(ent.Value, puddle.SolutionName, ref puddle.Solution, out var solution))
            {
                continue;
            }

            if (HasWaterReagent(solution, fire))
                return true;
        }

        _tileEntities.Clear();
        _lookup.GetLocalEntitiesIntersecting(gridUid, tile, _tileEntities, flags: LookupFlags.Uncontained, gridComp: grid);

        foreach (var ent in _tileEntities)
        {
            if (!IsWaterBlockingEntity(ent, itemQuery, xformQuery) ||
                !solutionQuery.TryGetComponent(ent, out var manager))
            {
                continue;
            }

            foreach (var (_, solutionEntity) in _solutionContainer.EnumerateSolutions((ent, manager)))
            {
                if (HasWaterReagent(solutionEntity.Comp.Solution, fire))
                {
                    _tileEntities.Clear();
                    return true;
                }
            }
        }

        _tileEntities.Clear();
        return false;
    }

    private bool IsWaterBlockingEntity(
        EntityUid uid,
        EntityQuery<ItemComponent> itemQuery,
        EntityQuery<TransformComponent> xformQuery)
    {
        if (itemQuery.HasComponent(uid))
            return false;

        return xformQuery.TryGetComponent(uid, out var xform) && xform.Anchored;
    }

    private bool HasWaterReagent(Solution solution, BrokenTechFireSpreadComponent fire)
    {
        foreach (var reagent in fire.WaterReagents)
        {
            if (solution.GetTotalPrototypeQuantity(reagent) > FixedPoint2.Zero)
                return true;
        }

        return false;
    }

    private bool HasWaterReagent(string reagentId, BrokenTechFireSpreadComponent fire)
    {
        foreach (var reagent in fire.WaterReagents)
        {
            if (reagent == reagentId)
                return true;
        }

        return false;
    }

    private bool HasBrokenTechFireAt(EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, tile);
        var fireQuery = GetEntityQuery<BrokenTechFireSpreadComponent>();

        while (anchored.MoveNext(out var ent))
        {
            if (fireQuery.HasComponent(ent))
                return true;
        }

        return false;
    }

    private readonly record struct SpreadRequest(
        EntityUid Uid,
        BrokenTechFireSpreadComponent Fire,
        EntityUid GridUid,
        MapGridComponent Grid,
        Vector2i Tile);
}
