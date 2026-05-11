using System.Numerics;
using Content.Server.DeadSpace.Lavaland.Components;
using Content.Server.Parallax;
using Content.Server.Tiles;
using Content.Shared.Chasm;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DeadSpace.Lavaland;
using Content.Shared.DeadSpace.Lavaland.Bosses;
using Content.Shared.FixedPoint;
using Content.Shared.Ghost;
using Content.Shared.Maps;
using Content.Shared.Mining.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Parallax.Biomes;
using Robust.Server.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Lavaland;

public sealed class LavalandBossArenaSystem : EntitySystem
{
    private const int DefaultArenaSize = 35;
    private const int MinArenaSize = 15;
    private const int MaxArenaSize = 65;
    private const int EntranceHalfWidth = 2;
    private const int EntranceDepth = 3;

    private static readonly TimeSpan ParticipantScanInterval = TimeSpan.FromSeconds(0.35);
    private static readonly TimeSpan HudUpdateInterval = TimeSpan.FromSeconds(0.25);
    private static readonly TimeSpan EmptyCleanupDelay = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan BossDeadCleanupDelay = TimeSpan.FromMinutes(3);
    private static readonly Vector2i[] RewardOffsets =
    {
        new(-1, 0),
        new(1, 0),
        new(0, 1),
        new(0, -1),
        new(-1, 1),
        new(1, 1),
        new(-1, -1),
        new(1, -1),
        Vector2i.Zero,
    };

    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinition = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TileSystem _tile = default!;

    private List<Entity<MapGridComponent>> _nearbyGrids = new();
    private readonly List<EntityUid> _anchoredToDelete = new();
    private readonly List<NetUserId> _leavingParticipants = new();
    private readonly List<(Vector2i Index, Tile Tile)> _reservedTiles = new();
    private readonly List<(Vector2i Index, Tile Tile)> _terrainTiles = new();
    private int _nextArenaId = 1;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LavalandBossComponent, DamageChangedEvent>(OnBossDamageChanged);
        SubscribeLocalEvent<LavalandBossComponent, MobStateChangedEvent>(OnBossMobStateChanged);
        SubscribeLocalEvent<LavalandBossArenaComponent, ComponentShutdown>(OnArenaShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<LavalandBossArenaComponent>();
        while (query.MoveNext(out var uid, out var arena))
        {
            if (arena.CleanupAt is { } cleanupAt && cleanupAt <= now)
            {
                CleanupArena((uid, arena));
                continue;
            }

            if (!arena.Ended && (!Exists(arena.Boss) || !Exists(arena.Grid)))
            {
                EndArena((uid, arena));
                continue;
            }

            if (arena.NextParticipantScan <= now)
            {
                arena.NextParticipantScan = now + ParticipantScanInterval;
                ScanParticipants((uid, arena), now);
            }

            if (!arena.Ended && arena.NextHudUpdate <= now)
            {
                arena.NextHudUpdate = now + HudUpdateInterval;
                SendHudUpdateToParticipants((uid, arena));
            }
        }
    }

    public void SpawnConfiguredArenas(
        EntityUid mapUid,
        MapGridComponent terrainGrid,
        BiomeComponent biome,
        LavalandPlanetPrototype planet,
        Random random)
    {
        foreach (var (arenaId, count) in planet.BossArenas)
        {
            if (!_prototype.TryIndex(arenaId, out var arenaPrototype))
            {
                Log.Error($"Lavaland boss arena cannot spawn: missing arena prototype {arenaId}.");
                continue;
            }

            for (var i = 0; i < Math.Max(0, count); i++)
            {
                TrySpawnBossArena(mapUid, terrainGrid, biome, planet, arenaPrototype, random);
            }
        }
    }

    public bool TrySpawnBossArena(
        EntityUid mapUid,
        MapGridComponent terrainGrid,
        BiomeComponent biome,
        LavalandPlanetPrototype planet,
        LavalandBossArenaPrototype arenaPrototype,
        Random random)
    {
        if (!_prototype.HasIndex<EntityPrototype>(arenaPrototype.BossPrototype.Id))
        {
            Log.Error($"Lavaland boss arena {arenaPrototype.ID} cannot spawn: missing boss prototype {arenaPrototype.BossPrototype}.");
            return false;
        }

        if (!_tileDefinition.TryGetDefinition(arenaPrototype.FloorTile, out var floorTile))
        {
            Log.Error($"Lavaland boss arena {arenaPrototype.ID} cannot spawn: missing floor tile {arenaPrototype.FloorTile}.");
            return false;
        }

        var width = NormalizeArenaSize(arenaPrototype.Size <= 0
            ? DefaultArenaSize
            : arenaPrototype.Size);
        var height = width;

        if (!TryFindArenaCenter(mapUid, terrainGrid, planet, arenaPrototype, random, width, height, out var center))
        {
            Log.Warning($"Lavaland boss arena {arenaPrototype.ID} failed to find a safe placement.");
            return false;
        }

        PrepareArenaTerrain(mapUid, terrainGrid, biome, floorTile, center, width, height);

        var mapId = Transform(mapUid).MapID;
        var grid = _mapManager.CreateGridEntity(mapId);
        _metadata.SetEntityName(grid.Owner, arenaPrototype.ArenaName);
        _transform.SetMapCoordinates(grid.Owner, new MapCoordinates(new Vector2(center.X, center.Y), mapId));

        FillArenaFloor(grid.Owner, grid.Comp, floorTile, arenaPrototype.FloorVisualPrototype, width, height);
        SpawnArenaWalls(grid.Owner, grid.Comp, arenaPrototype.WallPrototype, width, height);

        if (arenaPrototype.LightPrototype is { } lightPrototype)
            SpawnArenaLights(grid.Owner, grid.Comp, lightPrototype, width, height);

        var boss = Spawn(arenaPrototype.BossPrototype.Id, _map.GridTileToLocal(grid.Owner, grid.Comp, Vector2i.Zero));
        var bossComponent = EnsureComp<LavalandBossComponent>(boss);
        bossComponent.Arena = grid.Owner;
        bossComponent.MaxHealth = Math.Max(1f, bossComponent.MaxHealth);

        var arena = AddComp<LavalandBossArenaComponent>(grid.Owner);
        arena.ArenaId = _nextArenaId++;
        arena.Map = mapUid;
        arena.Grid = grid.Owner;
        arena.Boss = boss;
        arena.BossSpawnTile = Vector2i.Zero;
        arena.Width = width;
        arena.Height = height;
        arena.DeleteOnEmpty = arenaPrototype.DeleteOnEmpty;
        arena.DeleteOnBossDeath = arenaPrototype.DeleteOnBossDeath;
        arena.ReturnParticipantsOnDelete = arenaPrototype.ReturnParticipantsOnDelete;
        arena.ResetOnEmpty = arenaPrototype.ResetOnEmpty;
        arena.EmptyResetDelay = arenaPrototype.EmptyResetDelay < TimeSpan.Zero
            ? TimeSpan.Zero
            : arenaPrototype.EmptyResetDelay;
        arena.EmptyResetHealFraction = Math.Clamp(arenaPrototype.EmptyResetHealFraction, 0f, 1f);
        arena.EmptyResetMinHeal = Math.Max(0f, arenaPrototype.EmptyResetMinHeal);
        arena.BossName = string.IsNullOrWhiteSpace(bossComponent.BossName)
            ? Name(boss)
            : bossComponent.BossName;
        arena.MaxHealth = bossComponent.MaxHealth;
        arena.NextParticipantScan = _timing.CurTime;
        arena.NextHudUpdate = _timing.CurTime;

        Log.Info($"Lavaland boss arena {arenaPrototype.ID} spawned at {center}.");
        return true;
    }

    private bool TryFindArenaCenter(
        EntityUid mapUid,
        MapGridComponent terrainGrid,
        LavalandPlanetPrototype planet,
        LavalandBossArenaPrototype arenaPrototype,
        Random random,
        int width,
        int height,
        out Vector2i center)
    {
        center = default;

        var limit = GetPlacementLimit(planet, arenaPrototype, width, height);
        if (limit <= 0)
            return false;

        var arenaRadius = GetArenaRadius(width, height);
        var minDistance = Math.Min(Math.Max(0f, arenaPrototype.MinDistance), limit);
        var configuredMax = arenaPrototype.MaxDistance > 0f
            ? arenaPrototype.MaxDistance
            : limit;
        var maxDistance = Math.Clamp(configuredMax, minDistance, limit);
        var attempts = Math.Max(1, arenaPrototype.SpawnAttempts);

        for (var attempt = 0; attempt < attempts; attempt++)
        {
            var angle = random.NextDouble() * MathF.PI * 2f;
            var distance = minDistance + random.NextDouble() * (maxDistance - minDistance);
            var candidate = new Vector2i(
                (int) MathF.Round(MathF.Cos((float) angle) * (float) distance),
                (int) MathF.Round(MathF.Sin((float) angle) * (float) distance));

            if (IsInsideArenaExclusion(candidate, planet, arenaPrototype, arenaRadius) ||
                HasNearbyNonTerrainGrid(mapUid, candidate, arenaRadius + arenaPrototype.GridSeparation) ||
                HasPersistentAnchoredEntityInFootprint(mapUid, terrainGrid, candidate, width, height))
            {
                continue;
            }

            center = candidate;
            return true;
        }

        return false;
    }

    private void PrepareArenaTerrain(
        EntityUid mapUid,
        MapGridComponent terrainGrid,
        BiomeComponent biome,
        ITileDefinition floorTile,
        Vector2i center,
        int width,
        int height)
    {
        var bounds = GetReservationBounds(center, width, height);
        var reserveBounds = new Box2(bounds.Left, bounds.Bottom, bounds.Right, bounds.Top);

        _reservedTiles.Clear();
        _biome.ReserveTiles(mapUid, reserveBounds, _reservedTiles, biome, terrainGrid);
        _reservedTiles.Clear();

        _terrainTiles.Clear();
        for (var x = bounds.Left; x < bounds.Right; x++)
        {
            for (var y = bounds.Bottom; y < bounds.Top; y++)
            {
                _terrainTiles.Add((new Vector2i(x, y), CreateTile(floorTile)));
            }
        }

        _map.SetTiles(mapUid, terrainGrid, _terrainTiles);
        _terrainTiles.Clear();

        ClearClearableTerrainEntities(mapUid, terrainGrid, bounds);
    }

    private void FillArenaFloor(
        EntityUid gridUid,
        MapGridComponent grid,
        ITileDefinition floorTile,
        EntProtoId? floorVisualPrototype,
        int width,
        int height)
    {
        var halfWidth = width / 2;
        var halfHeight = height / 2;
        var tiles = new List<(Vector2i Index, Tile Tile)>(width * height + EntranceDepth * EntranceHalfWidth * 8);

        AddFloorRect(tiles, floorTile, -halfWidth, -halfHeight, halfWidth, halfHeight);
        AddFloorRect(tiles, floorTile, -EntranceHalfWidth, halfHeight + 1, EntranceHalfWidth, halfHeight + EntranceDepth);
        AddFloorRect(tiles, floorTile, -EntranceHalfWidth, -halfHeight - EntranceDepth, EntranceHalfWidth, -halfHeight - 1);
        AddFloorRect(tiles, floorTile, halfWidth + 1, -EntranceHalfWidth, halfWidth + EntranceDepth, EntranceHalfWidth);
        AddFloorRect(tiles, floorTile, -halfWidth - EntranceDepth, -EntranceHalfWidth, -halfWidth - 1, EntranceHalfWidth);

        _map.SetTiles(gridUid, grid, tiles);

        if (floorVisualPrototype == null)
            return;

        foreach (var (index, _) in tiles)
        {
            SpawnAnchored(floorVisualPrototype.Value.Id, gridUid, grid, index);
        }
    }

    private void SpawnArenaWalls(EntityUid gridUid, MapGridComponent grid, EntProtoId? wallPrototype, int width, int height)
    {
        if (wallPrototype == null)
            return;

        var halfWidth = width / 2;
        var halfHeight = height / 2;

        for (var x = -halfWidth; x <= halfWidth; x++)
        {
            if (Math.Abs(x) > EntranceHalfWidth)
            {
                SpawnAnchored(wallPrototype.Value.Id, gridUid, grid, new Vector2i(x, -halfHeight));
                SpawnAnchored(wallPrototype.Value.Id, gridUid, grid, new Vector2i(x, halfHeight));
            }
        }

        for (var y = -halfHeight + 1; y < halfHeight; y++)
        {
            if (Math.Abs(y) > EntranceHalfWidth)
            {
                SpawnAnchored(wallPrototype.Value.Id, gridUid, grid, new Vector2i(-halfWidth, y));
                SpawnAnchored(wallPrototype.Value.Id, gridUid, grid, new Vector2i(halfWidth, y));
            }
        }
    }

    private void SpawnArenaLights(EntityUid gridUid, MapGridComponent grid, EntProtoId lightPrototype, int width, int height)
    {
        var offsetX = Math.Max(EntranceHalfWidth + 2, width / 4);
        var offsetY = Math.Max(EntranceHalfWidth + 2, height / 4);

        SpawnAnchored(lightPrototype.Id, gridUid, grid, Vector2i.Zero);
        SpawnAnchored(lightPrototype.Id, gridUid, grid, new Vector2i(-offsetX, -offsetY));
        SpawnAnchored(lightPrototype.Id, gridUid, grid, new Vector2i(-offsetX, offsetY));
        SpawnAnchored(lightPrototype.Id, gridUid, grid, new Vector2i(offsetX, -offsetY));
        SpawnAnchored(lightPrototype.Id, gridUid, grid, new Vector2i(offsetX, offsetY));
    }

    private void ScanParticipants(Entity<LavalandBossArenaComponent> arena, TimeSpan now)
    {
        var current = new Dictionary<NetUserId, ICommonSession>();
        foreach (var session in _players.Sessions)
        {
            if (session.AttachedEntity is not { Valid: true } attached ||
                !Exists(attached) ||
                HasComp<GhostComponent>(attached) ||
                IsDead(attached) ||
                !TryComp(attached, out TransformComponent? xform) ||
                xform.GridUid != arena.Comp.Grid)
            {
                continue;
            }

            current[session.UserId] = session;
        }

        _leavingParticipants.Clear();
        foreach (var participant in arena.Comp.Participants)
        {
            if (!current.ContainsKey(participant))
                _leavingParticipants.Add(participant);
        }

        foreach (var participant in _leavingParticipants)
        {
            arena.Comp.Participants.Remove(participant);
            if (_players.TryGetSessionById(participant, out var session))
                SendHideAndStop(session, arena.Comp.ArenaId);
        }

        if (arena.Comp.Ended)
        {
            foreach (var (userId, session) in current)
            {
                if (arena.Comp.Participants.Add(userId))
                    SendHideAndStop(session, arena.Comp.ArenaId);
            }

            arena.Comp.EmptySince = null;
            return;
        }

        foreach (var (userId, session) in current)
        {
            if (arena.Comp.Participants.Add(userId))
            {
                if (arena.Comp.ReturnParticipantsOnDelete &&
                    session.AttachedEntity is { Valid: true } attached &&
                    !arena.Comp.ReturnCoordinates.ContainsKey(userId))
                {
                    arena.Comp.ReturnCoordinates[userId] = _transform.GetMapCoordinates(attached);
                }

                SendHudUpdate(session, arena.Comp);
                SendMusicStart(session, arena.Comp);
            }
        }

        if (arena.Comp.Participants.Count == 0)
        {
            arena.Comp.EmptySince ??= now;
            if (arena.Comp.ResetOnEmpty &&
                !arena.Comp.HasResetWhileEmpty &&
                now - arena.Comp.EmptySince >= arena.Comp.EmptyResetDelay)
            {
                ResetBoss((arena.Owner, arena.Comp), now);
                arena.Comp.HasResetWhileEmpty = true;
            }

            if (arena.Comp.DeleteOnEmpty && now - arena.Comp.EmptySince >= EmptyCleanupDelay)
                arena.Comp.CleanupAt = now;
        }
        else
        {
            arena.Comp.EmptySince = null;
            arena.Comp.HasResetWhileEmpty = false;
        }
    }

    private void SendHudUpdateToParticipants(Entity<LavalandBossArenaComponent> arena)
    {
        foreach (var participant in arena.Comp.Participants)
        {
            if (_players.TryGetSessionById(participant, out var session))
                SendHudUpdate(session, arena.Comp);
        }
    }

    private void SendHudUpdate(ICommonSession session, LavalandBossArenaComponent arena)
    {
        if (!Exists(arena.Boss))
        {
            SendHideAndStop(session, arena.ArenaId);
            return;
        }

        var currentHealth = GetCurrentBossHealth(arena.Boss, arena.MaxHealth);
        var ev = new LavalandBossHudUpdateEvent(
            arena.ArenaId,
            arena.BossName,
            currentHealth,
            arena.MaxHealth,
            arena.Participants.Count);
        RaiseNetworkEvent(ev, session.Channel);
    }

    private void SendMusicStart(ICommonSession session, LavalandBossArenaComponent arena)
    {
        if (!TryComp<LavalandBossComponent>(arena.Boss, out var boss) || boss.Music == null)
            return;

        var audioParams = boss.Music.Params.WithLoop(true);
        RaiseNetworkEvent(new LavalandBossMusicStartEvent(arena.ArenaId, _audio.ResolveSound(boss.Music), audioParams), session.Channel);
    }

    private void SendHideAndStop(ICommonSession session, int arenaId)
    {
        RaiseNetworkEvent(new LavalandBossHudHideEvent(arenaId), session.Channel);
        RaiseNetworkEvent(new LavalandBossMusicStopEvent(arenaId), session.Channel);
    }

    private void SendHideAndStopToParticipants(LavalandBossArenaComponent arena)
    {
        foreach (var participant in arena.Participants)
        {
            if (_players.TryGetSessionById(participant, out var session))
                SendHideAndStop(session, arena.ArenaId);
        }
    }

    private void OnBossDamageChanged(Entity<LavalandBossComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.Arena is not { Valid: true } arenaUid ||
            !TryComp<LavalandBossArenaComponent>(arenaUid, out var arena))
        {
            return;
        }

        SendHudUpdateToParticipants((arenaUid, arena));
    }

    private void OnBossMobStateChanged(Entity<LavalandBossComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead ||
            ent.Comp.Arena is not { Valid: true } arenaUid ||
            !TryComp<LavalandBossArenaComponent>(arenaUid, out var arena))
        {
            return;
        }

        if (ent.Comp.DeathSound != null)
            _audio.PlayPvs(ent.Comp.DeathSound, ent.Owner);

        if (!ent.Comp.DeathRewardsSpawned)
        {
            ent.Comp.DeathRewardsSpawned = true;
            SpawnDeathRewards(ent.Owner, ent.Comp);
        }

        QueueDel(ent.Owner);
        EndArena((arenaUid, arena));
    }

    private void SpawnDeathRewards(EntityUid bossUid, LavalandBossComponent boss)
    {
        if (boss.DeathRewards.Count == 0)
            return;

        if (!TryComp(bossUid, out TransformComponent? xform) ||
            xform.GridUid is not { } gridUid ||
            !TryComp<MapGridComponent>(gridUid, out var grid))
        {
            if (!TryComp(bossUid, out TransformComponent? fallbackXform))
                return;

            foreach (var reward in boss.DeathRewards)
            {
                if (_prototype.HasIndex<EntityPrototype>(reward.Id))
                    Spawn(reward.Id, fallbackXform.Coordinates);
            }

            return;
        }

        var center = _map.LocalToTile(gridUid, grid, xform.Coordinates);
        for (var i = 0; i < boss.DeathRewards.Count; i++)
        {
            var reward = boss.DeathRewards[i];
            if (!_prototype.HasIndex<EntityPrototype>(reward.Id))
            {
                Log.Error($"Lavaland boss {ToPrettyString(bossUid)} has missing death reward prototype {reward}.");
                continue;
            }

            var offset = RewardOffsets[Math.Min(i, RewardOffsets.Length - 1)];
            var tile = center + offset;
            if (!_map.TryGetTileRef(gridUid, grid, tile, out var tileRef) || tileRef.Tile.IsEmpty)
                tile = center;

            Spawn(reward.Id, _map.GridTileToLocal(gridUid, grid, tile));
        }
    }

    private void EndArena(Entity<LavalandBossArenaComponent> arena)
    {
        if (arena.Comp.Ended)
            return;

        arena.Comp.Ended = true;
        if (arena.Comp.DeleteOnBossDeath)
            arena.Comp.CleanupAt = _timing.CurTime + BossDeadCleanupDelay;

        SendHideAndStopToParticipants(arena.Comp);
    }

    private void CleanupArena(Entity<LavalandBossArenaComponent> arena)
    {
        if (arena.Comp.ReturnParticipantsOnDelete)
            ReturnParticipants(arena);

        SendHideAndStopToParticipants(arena.Comp);
        QueueDel(arena.Owner);
    }

    private void ResetBoss(Entity<LavalandBossArenaComponent> arena, TimeSpan now)
    {
        if (arena.Comp.Ended ||
            !Exists(arena.Comp.Boss) ||
            !TryComp<MapGridComponent>(arena.Comp.Grid, out var grid))
        {
            return;
        }

        _transform.SetCoordinates(arena.Comp.Boss, _map.GridTileToLocal(arena.Comp.Grid, grid, arena.Comp.BossSpawnTile));
        HealBossOnReset(arena.Comp);

        RaiseLocalEvent(arena.Comp.Boss, new LavalandBossResetEvent(arena.Owner, arena.Comp.BossSpawnTile));

        arena.Comp.NextHudUpdate = now;
    }

    private void HealBossOnReset(LavalandBossArenaComponent arena)
    {
        if (!TryComp<DamageableComponent>(arena.Boss, out var damageable))
            return;

        var totalDamage = damageable.TotalDamage.Float();
        if (totalDamage <= 0f)
            return;

        var heal = Math.Max(totalDamage * arena.EmptyResetHealFraction, arena.EmptyResetMinHeal);
        heal = Math.Clamp(heal, 0f, totalDamage);
        if (heal <= 0f)
            return;

        if (heal >= totalDamage - 0.01f)
        {
            _damageable.ClearAllDamage((arena.Boss, damageable));
            return;
        }

        _damageable.HealDistributed((arena.Boss, damageable), FixedPoint2.New(-heal), origin: arena.Boss);
    }

    private void ReturnParticipants(Entity<LavalandBossArenaComponent> arena)
    {
        foreach (var session in _players.Sessions)
        {
            if (session.AttachedEntity is not { Valid: true } attached ||
                !Exists(attached) ||
                !TryComp(attached, out TransformComponent? xform) ||
                xform.GridUid != arena.Comp.Grid)
            {
                continue;
            }

            if (arena.Comp.ReturnCoordinates.TryGetValue(session.UserId, out var coordinates))
                _transform.SetMapCoordinates(attached, coordinates);
        }
    }

    private void OnArenaShutdown(Entity<LavalandBossArenaComponent> ent, ref ComponentShutdown args)
    {
        SendHideAndStopToParticipants(ent.Comp);
    }

    private bool IsDead(EntityUid uid)
    {
        return TryComp(uid, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead;
    }

    private float GetCurrentBossHealth(EntityUid boss, float maxHealth)
    {
        if (!TryComp<DamageableComponent>(boss, out var damageable))
            return maxHealth;

        return Math.Clamp(maxHealth - damageable.TotalDamage.Float(), 0f, maxHealth);
    }

    private bool HasNearbyNonTerrainGrid(EntityUid terrainGridUid, Vector2 center, float radius)
    {
        _nearbyGrids.Clear();

        var mapId = Transform(terrainGridUid).MapID;
        var bounds = Box2.CenteredAround(center, Vector2.One * (radius * 2f + 1f));
        _mapManager.FindGridsIntersecting(mapId, bounds, ref _nearbyGrids);

        foreach (var grid in _nearbyGrids)
        {
            if (grid.Owner != terrainGridUid)
                return true;
        }

        return false;
    }

    private bool HasPersistentAnchoredEntityInFootprint(
        EntityUid terrainGridUid,
        MapGridComponent terrainGrid,
        Vector2i center,
        int width,
        int height)
    {
        var bounds = GetReservationBounds(center, width, height);

        for (var x = bounds.Left; x < bounds.Right; x++)
        {
            for (var y = bounds.Bottom; y < bounds.Top; y++)
            {
                var anchored = _map.GetAnchoredEntitiesEnumerator(terrainGridUid, terrainGrid, new Vector2i(x, y));
                while (anchored.MoveNext(out var uid))
                {
                    if (uid == null ||
                        TerminatingOrDeleted(uid.Value) ||
                        IsClearableTerrainEntity(uid.Value))
                    {
                        continue;
                    }

                    return true;
                }
            }
        }

        return false;
    }

    private void ClearClearableTerrainEntities(EntityUid terrainGridUid, MapGridComponent terrainGrid, Box2i bounds)
    {
        _anchoredToDelete.Clear();

        for (var x = bounds.Left; x < bounds.Right; x++)
        {
            for (var y = bounds.Bottom; y < bounds.Top; y++)
            {
                var anchored = _map.GetAnchoredEntitiesEnumerator(terrainGridUid, terrainGrid, new Vector2i(x, y));
                while (anchored.MoveNext(out var uid))
                {
                    if (uid != null &&
                        !TerminatingOrDeleted(uid.Value) &&
                        IsClearableTerrainEntity(uid.Value))
                    {
                        _anchoredToDelete.Add(uid.Value);
                    }
                }
            }
        }

        foreach (var uid in _anchoredToDelete)
        {
            if (!TerminatingOrDeleted(uid))
                QueueDel(uid);
        }

        _anchoredToDelete.Clear();
    }

    private bool IsClearableTerrainEntity(EntityUid uid)
    {
        if (HasComp<TileEntityEffectComponent>(uid) ||
            HasComp<ChasmComponent>(uid) ||
            HasComp<OreVeinComponent>(uid))
        {
            return true;
        }

        var prototypeId = MetaData(uid).EntityPrototype?.ID;
        return prototypeId is not null &&
               (prototypeId.StartsWith("WallRockBasalt", StringComparison.Ordinal) ||
                prototypeId is "FloorLavaEntity" or "FloorChasmEntity");
    }

    private static bool IsInsideArenaExclusion(
        Vector2 center,
        LavalandPlanetPrototype planet,
        LavalandBossArenaPrototype arenaPrototype,
        float arenaRadius)
    {
        var landingRadius = arenaPrototype.LandingExclusionRadius + arenaRadius;
        if (landingRadius > 0f && center.LengthSquared() <= landingRadius * landingRadius)
            return true;

        if (planet.TerminalReservationEnabled)
        {
            var outpostRadius = arenaPrototype.OutpostExclusionRadius + arenaRadius;
            if (outpostRadius > 0f &&
                Vector2.DistanceSquared(center, planet.TerminalGridOffset) <= outpostRadius * outpostRadius)
            {
                return true;
            }
        }

        if (planet.FtlEnabled)
        {
            var ftlRadius = arenaPrototype.FtlBeaconExclusionRadius + arenaRadius;
            if (ftlRadius > 0f &&
                Vector2.DistanceSquared(center, planet.FtlBeaconOffset) <= ftlRadius * ftlRadius)
            {
                return true;
            }
        }

        return false;
    }

    private static int GetPlacementLimit(LavalandPlanetPrototype planet, LavalandBossArenaPrototype arenaPrototype, int width, int height)
    {
        if (planet.MapHalfSize <= 0)
            return 0;

        var boundaryPadding = planet.BoundaryEnabled
            ? Math.Max(0, planet.BoundaryLavaWidth) + Math.Max(1, planet.BoundaryWallWidth)
            : 0;

        var arenaHalf = Math.Max(width, height) / 2 + EntranceDepth + 2;
        var padding = MathF.Ceiling(Math.Max(0f, arenaPrototype.MapEdgePadding)) + boundaryPadding + arenaHalf;
        return Math.Max(0, planet.MapHalfSize - (int) padding);
    }

    private static float GetArenaRadius(int width, int height)
    {
        var diameter = Math.Max(width, height) + EntranceDepth * 2;
        return diameter * 0.5f;
    }

    private static Box2i GetReservationBounds(Vector2i center, int width, int height)
    {
        var halfWidth = width / 2 + EntranceDepth;
        var halfHeight = height / 2 + EntranceDepth;
        return new Box2i(
            center.X - halfWidth,
            center.Y - halfHeight,
            center.X + halfWidth + 1,
            center.Y + halfHeight + 1);
    }

    private void AddFloorRect(
        List<(Vector2i Index, Tile Tile)> tiles,
        ITileDefinition tileDef,
        int minX,
        int minY,
        int maxX,
        int maxY)
    {
        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                tiles.Add((new Vector2i(x, y), CreateTile(tileDef)));
            }
        }
    }

    private Tile CreateTile(ITileDefinition tileDef)
    {
        return new Tile(tileDef.TileId,
            variant: tileDef is ContentTileDefinition contentTile
                ? _tile.PickVariant(contentTile)
                : (byte) 0);
    }

    private void SpawnAnchored(string prototype, EntityUid gridUid, MapGridComponent grid, Vector2i index)
    {
        var uid = Spawn(prototype, _map.GridTileToLocal(gridUid, grid, index));
        if (!TryComp(uid, out TransformComponent? xform) || xform.Anchored)
            return;

        _transform.AnchorEntity((uid, xform), (gridUid, grid), index);
    }

    private static int NormalizeArenaSize(int value)
    {
        value = Math.Clamp(value, MinArenaSize, MaxArenaSize);
        if (value % 2 == 0)
            value++;

        return Math.Min(value, MaxArenaSize);
    }
}
