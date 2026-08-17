// НИНТЕНДО ЗАСУДИТ БОИЧА И 50 ПРОЦЕНТОВ С ПРОДАЖ ДОМИНАТОРОВ ПОЙДУТ НА РАЗРАБОТКУ МАРИО
using System.Numerics;
using Content.Server.Decals;
using Content.Shared.Damage.Systems;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.Server.DeadSpace.Arena;

/// <summary>
/// Пеинтболл: hitscan-пули и гранаты ставят на пол цветные декали (по тайлам),
/// считается количество закрашенных тайлов каждой команды. Победа — по площади закраски.
/// </summary>
public sealed class PaintballSystem : EntitySystem
{
    [Dependency] private readonly DecalSystem _decals = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private const string PaintDecalId = "PaintballSplat";
    private const int MaxPaintedTiles = 1500;

    private EntityUid? _grid;
    private readonly Dictionary<Vector2i, uint> _decalsByTile = new();
    private readonly Dictionary<Vector2i, ArenaTeam> _ownerByTile = new();
    private int _blueTiles;
    private int _redTiles;

    /// <summary>Источник краски (команда/цвет) для взведённых гранат — ключ: граната.</summary>
    private readonly Dictionary<EntityUid, (ArenaTeam Team, Color Color)> _grenadePaintSource = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<PaintballProjectileComponent, HitscanRaycastFiredEvent>(OnPaintProjectileHit);
        SubscribeLocalEvent<PaintballGrenadeComponent, TriggerEvent>(OnPaintGrenadeTrigger);
        SubscribeLocalEvent<PaintballGrenadeComponent, ComponentShutdown>(OnPaintGrenadeShutdown);
    }

    /// <summary>
    /// Очищает все пятна краски и сбрасывает счёт. Вызывается при старте раунда пеинтболла,
    /// начале перерыва и перезапуске раунда.
    /// </summary>
    public void ResetPaint(EntityUid? mapUid)
    {
        _grid = FindArenaGrid(mapUid);
        if (_grid is { } grid && Exists(grid))
        {
            foreach (var decalId in _decalsByTile.Values)
                _decals.RemoveDecal(grid, decalId);
        }

        _decalsByTile.Clear();
        _ownerByTile.Clear();
        _blueTiles = 0;
        _redTiles = 0;
    }

    public int GetTeamTiles(ArenaTeam team)
    {
        return team switch
        {
            ArenaTeam.Blue => _blueTiles,
            ArenaTeam.Red => _redTiles,
            _ => 0,
        };
    }

    /// <summary>
    /// Закрашивает все тайлы в круге радиуса <paramref name="radius"/> вокруг точки в мировых координатах.
    /// </summary>
    private void PaintArea(EntityUid grid, Vector2 worldPos, float radius, Color color, ArenaTeam team)
    {
        if (!TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        var center = _maps.WorldToTile(grid, gridComp, worldPos);
        var r = (int)MathF.Ceiling(radius);
        for (var x = -r; x <= r; x++)
        {
            for (var y = -r; y <= r; y++)
            {
                var tile = center + new Vector2i(x, y);
                var tileWorld = _maps.GridTileToWorld(grid, gridComp, tile);
                if ((tileWorld.Position - worldPos).Length() > radius)
                    continue;
                PaintTile(grid, gridComp, tile, color, team);
            }
        }
    }

    private void PaintTile(EntityUid grid, MapGridComponent gridComp, Vector2i tile, Color color, ArenaTeam team)
    {
        if (team is not (ArenaTeam.Blue or ArenaTeam.Red))
            return;

        // Пустые тайлы (космос, дыры) не красим.
        if (_maps.GetTileRef(grid, gridComp, tile).Tile.IsEmpty)
            return;

        // Тайл уже закрашен нашей командой — ничего не делаем.
        if (_ownerByTile.TryGetValue(tile, out var owner) && owner == team)
            return;

        // Перекрашивание чужого тайла: снимаем старое пятно и очки у другой команды.
        if (_ownerByTile.TryGetValue(tile, out var oldOwner))
        {
            if (_decalsByTile.TryGetValue(tile, out var oldDecalId))
                _decals.RemoveDecal(grid, oldDecalId);
            ChangeCount(oldOwner, -1);
        }
        else if (_ownerByTile.Count >= MaxPaintedTiles)
        {
            return;
        }

        var coords = _maps.ToCenterCoordinates(grid, tile, gridComp);
        if (!_decals.TryAddDecal(PaintDecalId, coords, out var decalId, color: color, cleanable: false))
            return;

        _decalsByTile[tile] = decalId;
        _ownerByTile[tile] = team;
        ChangeCount(team, +1);
    }

    private void ChangeCount(ArenaTeam team, int delta)
    {
        if (team == ArenaTeam.Blue)
            _blueTiles = Math.Max(0, _blueTiles + delta);
        else if (team == ArenaTeam.Red)
            _redTiles = Math.Max(0, _redTiles + delta);
    }

    private void OnPaintProjectileHit(Entity<PaintballProjectileComponent> ent, ref HitscanRaycastFiredEvent args)
    {
        var data = args.Data;
        if (_grid is not { } grid)
            return;
        if (data.HitPosition is not { } hitPos)
            return;
        if (!ResolvePaintSource(data.Shooter ?? data.Gun, out var team, out var color))
            return;

        var dir = data.ShotDirection;
        if (dir.LengthSquared() <= 0f)
            return;
        dir = Vector2.Normalize(dir);

        // Начало шлейфа — от позиции стрелка до точки попадания.
        var start = data.Shooter is { Valid: true } shooter
            ? Transform(shooter).WorldPosition
            : hitPos.Position - dir * 45f;
        var end = hitPos.Position;

        PaintTrail(grid, start, end, ent.Comp.TrailWidth, color, team);

        if (ent.Comp.ImpactRadius > 0f)
            PaintArea(grid, end, ent.Comp.ImpactRadius, color, team);

        // Попадание по игроку — дополнительно пятно на его тайле.
        if (data.HitEntity is { Valid: true } hitEnt && HasComp<ArenaPlayerComponent>(hitEnt))
            PaintArea(grid, Transform(hitEnt).WorldPosition, 0.5f, color, team);
    }

    /// <summary>
    /// Красит тайлы вдоль отрезка полёта пули: все тайлы, чей центр находится в пределах
    /// <paramref name="width"/> от линии траектории.
    /// </summary>
    private void PaintTrail(EntityUid grid, Vector2 start, Vector2 end, float width, Color color, ArenaTeam team)
    {
        if (!TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        var tileA = _maps.WorldToTile(grid, gridComp, start);
        var tileB = _maps.WorldToTile(grid, gridComp, end);
        var minX = Math.Min(tileA.X, tileB.X) - 1;
        var maxX = Math.Max(tileA.X, tileB.X) + 1;
        var minY = Math.Min(tileA.Y, tileB.Y) - 1;
        var maxY = Math.Max(tileA.Y, tileB.Y) + 1;

        var widthSq = width * width;
        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var tile = new Vector2i(x, y);
                var tileWorld = _maps.GridTileToWorld(grid, gridComp, tile).Position;
                if (DistanceToSegmentSquared(tileWorld + new Vector2(0.5f, 0.5f), start, end) > widthSq)
                    continue;
                PaintTile(grid, gridComp, tile, color, team);
            }
        }
    }

    private static float DistanceToSegmentSquared(Vector2 point, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var lengthSq = ab.LengthSquared();
        if (lengthSq <= 0f)
            return (point - a).LengthSquared();
        var t = Math.Clamp(Vector2.Dot(point - a, ab) / lengthSq, 0f, 1f);
        var projection = a + ab * t;
        return (point - projection).LengthSquared();
    }

    private void OnPaintGrenadeTrigger(Entity<PaintballGrenadeComponent> ent, ref TriggerEvent args)
    {
        // Взведение (клик в руке или бросок, ключ "trigger"): запоминаем источник краски.
        if (args.Key != "timer")
        {
            if (ResolvePaintSource(args.User, out var team, out var color))
                _grenadePaintSource[ent] = (team, color);
            return;
        }

        // Реагируем только на детонацию таймера, а не на взведение гранаты в руке.
        if (!HasComp<ActiveTimerTriggerComponent>(ent))
            return;

        // Цвет краски: запомненный при взведении источник → бросивший игрок.
        if (!_grenadePaintSource.TryGetValue(ent, out var source) &&
            !ResolvePaintSource(args.User, out source.Team, out source.Color))
        {
            return;
        }

        // Решётка: сетка гранаты, иначе кэшированная сетка арены.
        var xform = Transform(ent);
        var pos = xform.WorldPosition;
        var mapId = xform.MapID;
        var grid = xform.GridUid is { Valid: true } g && HasComp<MapGridComponent>(g) ? g : _grid;
        if (grid is not { } gridUid)
            return;

        PaintArea(gridUid, pos, ent.Comp.PaintRadius, source.Color, source.Team);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg"), ent);

        var query = EntityQueryEnumerator<ArenaPlayerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var targetXform))
        {
            if (targetXform.MapID != mapId)
                continue;
            if ((targetXform.WorldPosition - pos).Length() > ent.Comp.DamageRadius)
                continue;
            _damage.TryChangeDamage(uid, ent.Comp.Damage);
        }

        QueueDel(ent);
    }

    private void OnPaintGrenadeShutdown(Entity<PaintballGrenadeComponent> ent, ref ComponentShutdown args)
    {
        _grenadePaintSource.Remove(ent);
    }

    /// <summary>
    /// Определяет команду и цвет краски источника: игрок с компонентом краски арены.
    /// </summary>
    private bool ResolvePaintSource(EntityUid? source, out ArenaTeam team, out Color color)
    {
        team = ArenaTeam.None;
        color = Color.White;
        if (source is { Valid: true } uid &&
            TryComp<ArenaPaintColorComponent>(uid, out var paint) &&
            paint.Team is ArenaTeam.Blue or ArenaTeam.Red)
        {
            team = paint.Team;
            color = paint.Color;
            return true;
        }

        return false;
    }

    private EntityUid? FindArenaGrid(EntityUid? mapUid)
    {
        if (mapUid == null || !Exists(mapUid.Value))
            return null;

        var mapId = Transform(mapUid.Value).MapID;
        var query = EntityQueryEnumerator<MapGridComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.MapID == mapId)
                return uid;
        }

        return null;
    }
}