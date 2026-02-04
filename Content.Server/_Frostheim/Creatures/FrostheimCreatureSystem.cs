using System.Numerics;
using Content.Server.NPC.HTN;
using Content.Shared._Frostheim.Creatures;
using Content.Shared._Frostheim.Map;
using Content.Shared._Frostheim.Roles;
using Content.Shared._Frostheim.Shuttle;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Frostheim.Creatures;

public sealed class FrostheimCreatureSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private const float InitialSpawnDelay = 90f;
    private const float RespawnCheckInterval = 30f;
    private const float MinPlayerDistance = 22f;
    private const float GuardSpawnRadius = 5f;
    private const int MaxSpawnAttempts = 30;

    private const int MaxSlashers = 5;
    private const int MaxLurkers = 2;
    private const int MaxSwarm = 6;

    private const string SlasherProto = "MobFrostSlasher";
    private const string LurkerProto = "MobFrostLurker";
    private const string SwarmProto = "MobFrostSwarm";
    private const string BruteProto = "MobFrostBrute";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostMapComponent, FrostheimWeatherChangedEvent>(OnWeatherChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FrostMapComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.LootSpawned)
                continue;

            if (!comp.CreaturesSpawned)
            {
                if (comp.CreatureSpawnTime == TimeSpan.Zero)
                {
                    comp.CreatureSpawnTime = _timing.CurTime + TimeSpan.FromSeconds(InitialSpawnDelay);
                    continue;
                }

                if (_timing.CurTime < comp.CreatureSpawnTime)
                    continue;

                SpawnInitialCreatures(uid, comp);
                comp.CreaturesSpawned = true;
                comp.NextRespawnCheck = _timing.CurTime + TimeSpan.FromSeconds(RespawnCheckInterval);
                continue;
            }

            if (_timing.CurTime < comp.NextRespawnCheck)
                continue;

            comp.NextRespawnCheck = _timing.CurTime + TimeSpan.FromSeconds(RespawnCheckInterval);
            CheckRespawns(uid, comp);
        }
    }

    private void SpawnInitialCreatures(EntityUid mapUid, FrostMapComponent comp)
    {
        var mapId = Transform(mapUid).MapID;
        var parts = GetBrokenParts(mapId);

        var slashersSpawned = 0;
        foreach (var (partUid, partXform) in parts)
        {
            if (slashersSpawned >= MaxSlashers)
                break;

            var partPos = _xform.GetWorldPosition(partXform);

            if (!TryFindSpawnPosition(mapId, partPos, GuardSpawnRadius, out var spawnCoords))
                continue;

            var mob = Spawn(SlasherProto, spawnCoords);
            SetupGuard(mob, guardXform: partXform);
            slashersSpawned++;
        }

        for (var i = 0; i < MaxLurkers; i++)
        {
            if (!TryFindRandomSpawnPosition(mapId, out var spawnCoords))
                continue;

            Spawn(LurkerProto, spawnCoords);
        }

        foreach (var (partUid, partXform) in parts)
        {
            if (!TryComp<FrostheimBrokenPartComponent>(partUid, out var partComp))
                continue;

            if (partComp.PartType != FrostheimPartType.Gyroscope)
                continue;

            var partPos = _xform.GetWorldPosition(partXform);
            if (!TryFindSpawnPosition(mapId, partPos, GuardSpawnRadius, out var spawnCoords))
                continue;

            var mob = Spawn(BruteProto, spawnCoords);
            SetupGuard(mob, guardXform: partXform);
            comp.BruteSpawned = true;
            break;
        }
    }

    private void SetupGuard(EntityUid mob, TransformComponent guardXform)
    {
        if (!TryComp<HTNComponent>(mob, out var htn))
            return;

        htn.Blackboard.SetValue("FollowTarget", guardXform.Coordinates);
    }

    private void CheckRespawns(EntityUid mapUid, FrostMapComponent comp)
    {
        var mapId = Transform(mapUid).MapID;

        var slashers = 0;
        var lurkers = 0;
        var swarm = 0;
        var brutes = 0;

        var creatureQuery = EntityQueryEnumerator<FrostheimCreatureComponent, MobStateComponent, TransformComponent>();
        while (creatureQuery.MoveNext(out _, out var creature, out var mobState, out var xform))
        {
            if (xform.MapID != mapId)
                continue;

            if (mobState.CurrentState != MobState.Alive)
                continue;

            switch (creature.CreatureType)
            {
                case FrostheimCreatureType.Slasher:
                    slashers++;
                    break;
                case FrostheimCreatureType.Lurker:
                    lurkers++;
                    break;
                case FrostheimCreatureType.Swarm:
                    swarm++;
                    break;
                case FrostheimCreatureType.Brute:
                    brutes++;
                    break;
            }
        }

        if (slashers < MaxSlashers)
        {
            var parts = GetBrokenParts(mapId);
            foreach (var (partUid, partXform) in parts)
            {
                if (slashers >= MaxSlashers)
                    break;

                if (HasGuardNearby(partXform, mapId))
                    continue;

                var partPos = _xform.GetWorldPosition(partXform);
                if (!TryFindSpawnPosition(mapId, partPos, GuardSpawnRadius, out var spawnCoords))
                    continue;

                var mob = Spawn(SlasherProto, spawnCoords);
                SetupGuard(mob, guardXform: partXform);
                slashers++;
                break;
            }
        }

        if (lurkers < MaxLurkers)
        {
            if (TryFindRandomSpawnPosition(mapId, out var spawnCoords))
            {
                Spawn(LurkerProto, spawnCoords);
            }
        }

        if (swarm < MaxSwarm)
        {
            var toSpawn = Math.Min(2, MaxSwarm - swarm);
            for (var i = 0; i < toSpawn; i++)
            {
                if (!TryFindRandomSpawnPosition(mapId, out var spawnCoords))
                    continue;

                Spawn(SwarmProto, spawnCoords);
            }
        }

        if (brutes < 1 && comp.BruteSpawned)
        {
            var parts = GetBrokenParts(mapId);
            foreach (var (partUid, partXform) in parts)
            {
                if (!TryComp<FrostheimBrokenPartComponent>(partUid, out var partComp))
                    continue;

                if (partComp.PartType != FrostheimPartType.Gyroscope)
                    continue;

                var partPos = _xform.GetWorldPosition(partXform);
                if (!TryFindSpawnPosition(mapId, partPos, GuardSpawnRadius, out var spawnCoords))
                    continue;

                var mob = Spawn(BruteProto, spawnCoords);
                SetupGuard(mob, guardXform: partXform);
                break;
            }
        }
    }

    private bool HasGuardNearby(TransformComponent partXform, MapId mapId)
    {
        var partPos = _xform.GetWorldPosition(partXform);

        var creatureQuery = EntityQueryEnumerator<FrostheimCreatureComponent, MobStateComponent, TransformComponent>();
        while (creatureQuery.MoveNext(out _, out var creature, out var mobState, out var xform))
        {
            if (xform.MapID != mapId)
                continue;

            if (mobState.CurrentState != MobState.Alive)
                continue;

            if (creature.CreatureType != FrostheimCreatureType.Slasher)
                continue;

            var creaturePos = _xform.GetWorldPosition(xform);
            if (Vector2.Distance(partPos, creaturePos) < 15f)
                return true;
        }

        return false;
    }

    private void OnWeatherChanged(EntityUid uid, FrostMapComponent comp, ref FrostheimWeatherChangedEvent args)
    {
        if (args.NewWeather is not (FrostheimWeather.SnowfallMedium or FrostheimWeather.SnowfallHeavy))
            return;

        if (!comp.CreaturesSpawned)
            return;

        var mapId = Transform(uid).MapID;

        var currentSwarm = 0;
        var swarmQuery = EntityQueryEnumerator<FrostheimCreatureComponent, MobStateComponent, TransformComponent>();
        while (swarmQuery.MoveNext(out _, out var creature, out var mobState, out var xform))
        {
            if (xform.MapID != mapId)
                continue;

            if (mobState.CurrentState != MobState.Alive)
                continue;

            if (creature.CreatureType == FrostheimCreatureType.Swarm)
                currentSwarm++;
        }

        var toSpawn = Math.Min(3, MaxSwarm - currentSwarm);

        for (var i = 0; i < toSpawn; i++)
        {
            if (!TryFindRandomSpawnPosition(mapId, out var spawnCoords))
                continue;

            Spawn(SwarmProto, spawnCoords);
        }
    }

    private List<(EntityUid Uid, TransformComponent Xform)> GetBrokenParts(MapId mapId)
    {
        var parts = new List<(EntityUid, TransformComponent)>();

        var partQuery = EntityQueryEnumerator<FrostheimBrokenPartComponent, TransformComponent>();
        while (partQuery.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.MapID != mapId)
                continue;

            parts.Add((uid, xform));
        }

        return parts;
    }

    private bool TryFindSpawnPosition(MapId mapId, Vector2 center, float radius, out EntityCoordinates coords)
    {
        coords = default;

        for (var i = 0; i < MaxSpawnAttempts; i++)
        {
            var offset = _random.NextVector2(radius);
            var worldPos = center + offset;

            if (!IsValidSpawnPosition(mapId, worldPos, out coords))
                continue;

            return true;
        }

        return false;
    }

    private bool TryFindRandomSpawnPosition(MapId mapId, out EntityCoordinates coords)
    {
        coords = default;

        var parts = GetBrokenParts(mapId);
        if (parts.Count == 0)
            return false;

        for (var i = 0; i < MaxSpawnAttempts; i++)
        {
            var (_, partXform) = _random.Pick(parts);
            var partPos = _xform.GetWorldPosition(partXform);
            var offset = _random.NextVector2(15f);
            var worldPos = partPos + offset;

            if (!IsValidSpawnPosition(mapId, worldPos, out coords))
                continue;

            return true;
        }

        return false;
    }

    private bool IsValidSpawnPosition(MapId mapId, Vector2 worldPos, out EntityCoordinates coords)
    {
        coords = default;

        var gridQuery = EntityQueryEnumerator<MapGridComponent, TransformComponent>();
        while (gridQuery.MoveNext(out var gridUid, out var grid, out var gridXform))
        {
            if (gridXform.MapID != mapId)
                continue;

            if (HasComp<FrostheimShuttleComponent>(gridUid))
                continue;

            var localPos = Vector2.Transform(worldPos, _xform.GetInvWorldMatrix(gridXform));
            var tileIndices = new Vector2i((int) Math.Floor(localPos.X), (int) Math.Floor(localPos.Y));

            if (!_mapSystem.TryGetTileRef(gridUid, grid, tileIndices, out var tileRef))
                continue;

            if (tileRef.Tile.IsEmpty)
                continue;

            if (_turf.IsTileBlocked(gridUid, tileIndices, CollisionGroup.MobMask))
                continue;

            if (!IsFarFromPlayers(worldPos, mapId))
                continue;

            coords = new EntityCoordinates(gridUid, localPos);
            return true;
        }

        return false;
    }

    private bool IsFarFromPlayers(Vector2 worldPos, MapId mapId)
    {
        var playerQuery = EntityQueryEnumerator<FrostheimCrewComponent, TransformComponent>();
        while (playerQuery.MoveNext(out _, out _, out var xform))
        {
            if (xform.MapID != mapId)
                continue;

            var playerPos = _xform.GetWorldPosition(xform);
            if (Vector2.Distance(worldPos, playerPos) < MinPlayerDistance)
                return false;
        }

        return true;
    }
}
