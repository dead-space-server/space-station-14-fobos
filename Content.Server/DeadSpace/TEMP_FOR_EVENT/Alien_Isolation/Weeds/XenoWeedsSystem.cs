using Content.Server.Atmos.Components;
using Content.Shared.DeadSpace.TEMP_FOR_EVENT.Alien_Isolation.Weeds;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Spawners;

namespace Content.Server.DeadSpace.TEMP_FOR_EVENT.Alien_Isolation.Weeds;

public sealed class XenoWeedsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private static readonly EntProtoId XenoWallVisualId = "EventXenoWeedWall";

    private const string WallTag = "Wall";
    private static readonly Direction[] Directions =
    {
        Direction.East, Direction.West, Direction.North, Direction.South
    };

    private static readonly Direction[] AllDirections =
    {
        Direction.East, Direction.West, Direction.North, Direction.South,
        Direction.NorthEast, Direction.NorthWest, Direction.SouthEast, Direction.SouthWest
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WeedComponent, MapInitEvent>(OnWeedsMapInit);
        SubscribeLocalEvent<WeedComponent, EntityTerminatingEvent>(OnWeedsTerminating);
    }

    private void OnWeedsMapInit(EntityUid uid, WeedComponent comp, MapInitEvent args)
    {
        if (comp.IsSource)
            EnsureSpreading(uid);
    }

    private void EnsureSpreading(EntityUid uid)
    {
        var spreading = EnsureComp<WeedSpreadingComponent>(uid);
        spreading.SpreadAt = _timing.CurTime + spreading.SpreadDelay;
    }

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<WeedSpreadingComponent, WeedComponent, TransformComponent>();
        var toSpawn = new List<(EntityUid Parent, WeedComponent ParentComp, EntityUid Grid, MapGridComponent GridComp, Vector2i Tile)>();

        while (query.MoveNext(out var uid, out var spreading, out var weeds, out var xform))
        {
            if (curTime < spreading.SpreadAt)
                continue;

            RemCompDeferred<WeedSpreadingComponent>(uid);

            if (xform.GridUid is not { } gridId || !TryComp<MapGridComponent>(gridId, out var grid))
                continue;

            var indices = _mapSystem.TileIndicesFor(gridId, grid, xform.Coordinates);

            foreach (var dir in Directions)
            {
                var targetTile = indices.Offset(dir);
                if (TrySpreadToTile(uid, weeds, gridId, grid, targetTile))
                {
                    toSpawn.Add((uid, weeds, gridId, grid, targetTile));
                }
            }
        }

        foreach (var task in toSpawn)
        {
            CreateWeeds(task.Parent, task.ParentComp, task.Grid, task.GridComp, task.Tile);
        }
    }

    private bool TrySpreadToTile(EntityUid sourceUid, WeedComponent weeds, EntityUid gridId, MapGridComponent grid, Vector2i tile)
    {
        if (!_mapSystem.TryGetTileRef(gridId, grid, tile, out var tileRef) || tileRef.Tile.IsEmpty || _turf.IsSpace(tileRef))
            return false;

        var anchored = _mapSystem.GetAnchoredEntitiesEnumerator(gridId, grid, tile);
        EntityUid? weakerWeeds = null;

        while (anchored.MoveNext(out var anchoredId))
        {
            if (TryComp<AirtightComponent>(anchoredId, out var airtight) && airtight.AirBlocked)
                return false;

            if (TryComp<PhysicsComponent>(anchoredId, out var phys) &&
                (phys.CollisionLayer & (int)CollisionGroup.Impassable) != 0)
                return false;

            if (TryComp<WeedComponent>(anchoredId, out var otherWeeds))
            {
                if (otherWeeds.Level >= weeds.Level)
                    return false;

                weakerWeeds = anchoredId;
            }
        }

        var source = weeds.IsSource ? sourceUid : weeds.Source;

        if (!Exists(source))
            return false;

        if (TryComp<TransformComponent>(source, out var sourceXform))
        {
            var sourceTile = _mapSystem.TileIndicesFor(gridId, grid, sourceXform.Coordinates);
            var dist = (tile - sourceTile).Length;
            if (dist > weeds.Range)
                return false;
        }

        if (weakerWeeds != null)
            QueueDel(weakerWeeds.Value);

        return true;
    }

    private void SetupWeedComponent(EntityUid uid, EntityUid parentUid, WeedComponent parentWeeds)
    {
        // Добавляем или берем компонент сорняков на новой сущности
        var childWeeds = EnsureComp<WeedComponent>(uid);

        childWeeds.IsSource = false;
        // Если родитель был источником, то источником для ребенка станет сам родитель.
        // Если родитель уже был "ребенком", берем его изначальный источник.
        childWeeds.Source = parentWeeds.IsSource ? parentUid : parentWeeds.Source;
        childWeeds.Range = parentWeeds.Range;
        childWeeds.Level = parentWeeds.Level;

        // Запускаем таймер роста для этой новой части сорняков
        EnsureSpreading(uid);

        // Добавляем ссылку в список источника для массового удаления в будущем
        if (childWeeds.Source is { } source && TryComp<WeedComponent>(source, out var sourceWeeds))
        {
            sourceWeeds.Spread.Add(uid);
        }
    }

    private void CheckTileForWalls(Vector2i tile, EntityUid gridId, MapGridComponent grid, List<EntityUid> list)
    {
        var anchored = _mapSystem.GetAnchoredEntitiesEnumerator(gridId, grid, tile);
        while (anchored.MoveNext(out var ent))
        {
            if (_tagSystem.HasTag(ent.Value, WallTag) && !HasComp<XenoWeedWallComponent>(ent.Value))
            {
                list.Add(ent.Value);
            }
        }
    }

    private void CreateWeeds(EntityUid parentUid, WeedComponent parentWeeds, EntityUid gridId, MapGridComponent grid, Vector2i tile)
    {
        var coords = _mapSystem.GridTileToLocal(gridId, grid, tile);

        // 1. Спавним сорняк на пол
        var child = Spawn(parentWeeds.Spawns, coords);
        SetupWeedComponent(child, parentUid, parentWeeds);

        // 2. Собираем стены
        var wallsToInfect = new List<EntityUid>();

        // Проверяем саму плитку (центр)
        CheckTileForWalls(tile, gridId, grid, wallsToInfect);

        // Проверяем все 8 направлений из вашего массива
        foreach (var dir in AllDirections)
        {
            var neighborTile = tile.Offset(dir);
            CheckTileForWalls(neighborTile, gridId, grid, wallsToInfect);
        }

        // 3. Спавним налет
        foreach (var wall in wallsToInfect)
        {
            SpawnWallCover(wall, parentUid, parentWeeds);
        }
    }

    private void SpawnWallCover(EntityUid wallUid, EntityUid parentUid, WeedComponent parentWeeds)
    {
        var xform = Transform(wallUid);
        if (xform.GridUid == null)
            return;

        var weedWall = Spawn(XenoWallVisualId, xform.Coordinates);

        // Записываем стену в компонент сорняка, чтобы потом "почистить" её
        var childWeeds = EnsureComp<WeedComponent>(weedWall);
        childWeeds.AttachedWall = wallUid;

        EnsureComp<XenoWeedWallComponent>(wallUid);
        SetupWeedComponent(weedWall, parentUid, parentWeeds);
    }

    private void OnWeedsTerminating(EntityUid uid, WeedComponent component, ref EntityTerminatingEvent args)
    {
        if (component.AttachedWall is { } wall && Exists(wall))
        {
            RemCompDeferred<XenoWeedWallComponent>(wall);
        }

        if (!component.IsSource && component.Source is { } source && TryComp<WeedComponent>(source, out var sourceComp))
        {
            sourceComp.Spread.Remove(uid);
        }

        if (component.IsSource)
        {
            foreach (var child in component.Spread)
            {
                if (!Exists(child) || Terminating(child))
                    continue;

                var timer = EnsureComp<TimedDespawnComponent>(child);
                timer.Lifetime = _random.NextFloat(5f, 8.0f);
            }

            component.Spread.Clear();
        }
    }
}
