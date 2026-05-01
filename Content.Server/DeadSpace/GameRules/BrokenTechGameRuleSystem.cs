// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using System.Numerics;
using Content.Server.Explosion.EntitySystems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.DeadSpace.GameRules.Components;
using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors; 
using Content.Shared.GameTicking.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Random;

namespace Content.Server.DeadSpace.GameRules;

public sealed class BrokenTechGameRuleSystem : GameRuleSystem<BrokenTechGameRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    
        var query = EntityQueryEnumerator<BrokenTechGameRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var ruleComp, out var gameRule))
        {
            if (!GameTicker.IsGameRuleActive(uid, gameRule))
                continue;
    
            foreach (var entry in ruleComp.ListComponent)
            {
                entry.ElapsedSeconds += frameTime;
                var maxSeconds = entry.MinuteMax * 60f;
    
                if (entry.ElapsedSeconds < entry.NextAttemptSeconds && entry.ElapsedSeconds < maxSeconds)
                    continue;
    
                bool forceTrigger = entry.ElapsedSeconds >= maxSeconds;
    
                if (!forceTrigger)
                {
                    if (_random.Next(100) >= entry.Chance)
                    {
                        var remaining = maxSeconds - entry.ElapsedSeconds;
                        entry.NextAttemptSeconds = remaining > 1f
                            ? entry.ElapsedSeconds + _random.NextFloat(1f, remaining)
                            : maxSeconds;
                        continue;
                    }
                }
    
                ExecuteEntry(entry);
    
                entry.ElapsedSeconds = 0f;
                var minSec = entry.MinuteMin * 60f;
                var maxSec = entry.MinuteMax * 60f;
                entry.NextAttemptSeconds = _random.NextFloat(minSec, maxSec);
            }
        }
    }

    protected override void Started(EntityUid uid, BrokenTechGameRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        foreach (var entry in component.ListComponent)
        {
            entry.ElapsedSeconds = 0f;
            entry.Triggered = false;
            var minSeconds = entry.MinuteMin * 60f;
            var maxSeconds = entry.MinuteMax * 60f;
            entry.NextAttemptSeconds = _random.NextFloat(minSeconds, maxSeconds);
        }
    }

    private void ExecuteEntry(BrokenTechEntry entry)
    {
        var entities = GetEntitiesWithComponent(entry.ComponentName);
        if (entities.Count == 0)
            return;

        _random.Shuffle(entities);

        var targets = entities
            .Distinct()
            .Take(entry.HowMuchEntity)
            .ToList();

        foreach (var target in targets)
        {
            switch (entry.Action)
            {
                case ExplodeEntityAction explode:
                    HandleExplode(target, explode);
                    break;
                case BlockWorkingEntityAction block:
                    HandleBlock(target, block);
                    break;
            }
        }
    }

    private List<EntityUid> GetEntitiesWithComponent(string componentName)
    {
        var result = new List<EntityUid>();
        if (!_compFactory.TryGetRegistration(componentName, out var registration))
            return result;

        foreach (var comp in EntityManager.GetAllComponents(registration.Type))
        {
            result.Add(comp.Uid);
        }

        return result;
    }

    private void HandleExplode(EntityUid uid, ExplodeEntityAction action)
    {
        if (TerminatingOrDeleted(uid))
            return;
    
        SpawnFromTable(uid, action.SpawnTable);
        _explosion.QueueExplosion(uid, action.ExplosionType, action.ExplosionIntensity, 1f, 2f, maxTileBreak: 0);
        QueueDel(uid);
    }
    
    private void HandleBlock(EntityUid uid, BlockWorkingEntityAction action)
    {
        if (Deleted(uid))
            return;
    
        SpawnFromTable(uid, action.SpawnTable);
    
        if (_compFactory.TryGetRegistration("NodeContainer", out var nodeReg)
            && EntityManager.HasComponent(uid, nodeReg.Type))
        {
            EntityManager.RemoveComponent(uid, nodeReg.Type);
            return;
        }
    
        QueueDel(uid);
    }

    private void SpawnFromTable(EntityUid anchor, EntityTableSelector? selector)
    {
        if (selector == null)
            return;

        var spawns = _entityTable.GetSpawns(selector);
        if (spawns == null)
            return;

        var xform = Transform(anchor);
        var spawnPos = FindAtmosphericNeighbor(anchor, xform.GridUid);
        if (spawnPos == null)
            return;

        foreach (var proto in spawns)
        {
            Spawn(proto, new MapCoordinates(spawnPos.Value, xform.MapID));
        }
    }

    private Vector2? FindAtmosphericNeighbor(EntityUid anchor, EntityUid? gridUid)
    {
        if (gridUid is not { } gridEnt || !TryComp<MapGridComponent>(gridEnt, out var grid))
            return null;

        var worldPos = _transform.GetWorldPosition(anchor);
        var tilePos = _mapSystem.WorldToTile(gridEnt, grid, worldPos);
        var mapUid = Transform(gridEnt).MapUid;

        var neighbors = new[]
        {
            tilePos + new Vector2i(0, -1),
            tilePos + new Vector2i(0, 1),
            tilePos + new Vector2i(1, 0),
            tilePos + new Vector2i(-1, 0),
        };

        foreach (var neighbor in neighbors)
        {
            var tile = _mapSystem.GetTileRef(gridEnt, grid, neighbor);
            if (tile.Tile.IsEmpty)
                continue;

            var mixture = _atmos.GetTileMixture(gridEnt, mapUid, neighbor);
            if (mixture != null && mixture.TotalMoles > 0)
                return _mapSystem.GridTileToWorldPos(gridEnt, grid, neighbor);
        }

        return null;
    }
}