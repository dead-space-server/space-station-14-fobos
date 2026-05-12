using System.Numerics;
using Content.Server.DeadSpace.Lavaland.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Random.Helpers;
using Content.Shared.Throwing;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Lavaland;

public sealed class LavalandBubblegumSystem : EntitySystem
{
    private static readonly Vector2i[] Cardinals =
    {
        new(0, 1),
        new(1, 0),
        new(0, -1),
        new(-1, 0),
    };

    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    private readonly List<EntityUid> _participants = new();
    private readonly List<EntityUid> _bloodTargets = new();
    private readonly List<Vector2i> _poolTiles = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LavalandBubblegumComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<LavalandBubblegumComponent, LavalandBossFightStartedEvent>(OnBossFightStarted);
        SubscribeLocalEvent<LavalandBubblegumComponent, LavalandBossResetEvent>(OnBossReset);
        SubscribeLocalEvent<LavalandBubblegumComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<LavalandBubblegumComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<LavalandBubblegumComponent, LavalandBossComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var bubblegum, out var boss, out var xform))
        {
            if (boss.Arena is not { Valid: true } arenaUid ||
                !TryComp<LavalandBossArenaComponent>(arenaUid, out var arena) ||
                arena.Ended ||
                !arena.FightStarted ||
                xform.GridUid != arena.Grid ||
                !TryComp<MapGridComponent>(arena.Grid, out var grid) ||
                IsDead(uid))
            {
                ClearRuntimeState(uid, bubblegum, false);
                continue;
            }

            PruneTracked(bubblegum);
            ProcessPendingBloodTiles(bubblegum, arena, arena.Grid, grid, now);
            ProcessPendingHandAttacks(uid, bubblegum, arena, arena.Grid, grid, now);

            var participantCount = CollectParticipants(arena);
            if (participantCount == 0)
            {
                ClearRuntimeState(uid, bubblegum, true);
                bubblegum.BusyUntil = TimeSpan.Zero;
                continue;
            }

            if (ProcessCharge(uid, bubblegum, arena, arena.Grid, grid, now) ||
                ProcessQueuedCharge(uid, bubblegum, arena, arena.Grid, grid, now))
            {
                continue;
            }

            var bloodReactionWindow = GetBloodReactionWindow(bubblegum);
            if (bubblegum.BusyUntil <= now &&
                bubblegum.NextAttack - now > bloodReactionWindow &&
                bubblegum.NextBloodReaction <= now &&
                TryQueueBloodAttack(uid, bubblegum, arena, arena.Grid, grid, now))
            {
                bubblegum.LastPressureAt = now;
                bubblegum.LastAttackKind = "blood-reaction";
                bubblegum.NextBloodReaction = now + bubblegum.BloodReactionCooldown;
                continue;
            }

            if (bubblegum.BusyUntil > now ||
                bubblegum.NextAttack > now)
            {
                continue;
            }

            var target = PickTarget(bubblegum, uid, arena.Grid, grid, now);
            if (target == null)
                continue;

            RunAttack(uid, bubblegum, arena, arena.Grid, grid, target.Value, now);
        }
    }

    private void OnDamageChanged(Entity<LavalandBubblegumComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased ||
            args.DamageDelta == null ||
            args.DamageDelta.GetTotal() <= 0 ||
            !_random.Prob(0.25f) ||
            !TryComp<LavalandBossComponent>(ent.Owner, out var boss) ||
            boss.Arena is not { Valid: true } arenaUid ||
            !TryComp<LavalandBossArenaComponent>(arenaUid, out var arena) ||
            !TryComp<MapGridComponent>(arena.Grid, out var grid))
        {
            return;
        }

        var tile = GetEntityTile(ent.Owner, arena.Grid, grid);
        if (tile == null)
            return;

        if (_random.Prob(0.4f))
            tile += _random.Pick(Cardinals);

        if (IsInsideInnerArena(arena, tile.Value))
        {
            TrySpawnBloodPool(ent.Comp, arena, arena.Grid, grid, tile.Value);
            SpawnAnchored(ent.Comp.BloodGibsPrototype, arena.Grid, grid, tile.Value);
        }
    }

    private void OnBossReset(EntityUid uid, LavalandBubblegumComponent component, LavalandBossResetEvent args)
    {
        PrepareFight(uid, component);
    }

    private void OnBossFightStarted(EntityUid uid, LavalandBubblegumComponent component, LavalandBossFightStartedEvent args)
    {
        PrepareFight(uid, component);
    }

    private void PrepareFight(EntityUid uid, LavalandBubblegumComponent component)
    {
        ClearRuntimeState(uid, component, true);
        var now = _timing.CurTime;
        component.NextAttack = now + TimeSpan.FromSeconds(1);
        component.NextBloodReaction = now + TimeSpan.FromSeconds(1.5);
        component.LastPressureAt = now;
        component.LastAttackKind = string.Empty;
    }

    private void OnMobStateChanged(Entity<LavalandBubblegumComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            ClearRuntimeState(ent.Owner, ent.Comp, false);
    }

    private void OnRefreshMovementSpeed(EntityUid uid, LavalandBubblegumComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (component.Charging)
        {
            args.ModifySpeed(0f, 0f);
            return;
        }

        var rage = CalculateRage(uid);
        var modifier = Math.Clamp(1f + rage * 0.025f, 1f, 1.5f);
        args.ModifySpeed(modifier, modifier);
    }

    private void RunAttack(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        EntityUid target,
        TimeSpan now)
    {
        var rage = CalculateRage(boss);
        var belowHalf = IsBelowHalfHealth(boss);
        var targetTile = GetEntityTile(target, gridUid, grid);
        var bossTile = GetEntityTile(boss, gridUid, grid);
        if (targetTile == null || bossTile == null)
        {
            bubblegum.NextAttack = now + TimeSpan.FromSeconds(0.5);
            return;
        }

        var forcePressure = NeedsPressure(bubblegum, now);
        var pressureTarget = PickSecondaryTarget(bubblegum, target, gridUid, grid, now) ?? target;
        var pressureTargetTile = GetEntityTile(pressureTarget, gridUid, grid) ?? targetTile.Value;
        var pressureDistance = ChebyshevDistance(bossTile.Value, pressureTargetTile);
        var targetHasBlood = HasBloodPoolWithin(bubblegum, gridUid, pressureTargetTile, 1);
        var forcedAmbush = false;

        if (forcePressure || (!targetHasBlood && pressureDistance > 4))
        {
            forcedAmbush = QueueBloodPressureAtTarget(bubblegum, arena, gridUid, grid, pressureTargetTile, now, forcePressure);
            if (forcedAmbush)
            {
                MarkPressure(bubblegum, now, forcePressure ? "forced-blood-pressure" : "blood-pressure", pressureTarget);
                bubblegum.BusyUntil = now + bubblegum.BloodSmackDelay + bubblegum.BloodHandRecover;
                bubblegum.NextAttack = now + GetScaledCooldown(bubblegum.RangedCooldown, rage);
                bubblegum.NextBloodReaction = now + bubblegum.BloodReactionCooldown;
                return;
            }
        }

        var didBloodAttack = !forcedAmbush && TryQueueBloodAttack(boss, bubblegum, arena, gridUid, grid, now);
        if (didBloodAttack)
        {
            MarkPressure(bubblegum, now, "blood-hand", target);
            bubblegum.NextAttack = now + GetScaledCooldown(bubblegum.RangedCooldown, rage);
            bubblegum.NextBloodReaction = now + bubblegum.BloodReactionCooldown;
            return;
        }

        var warped = false;
        if (!didBloodAttack)
        {
            QueueBloodSpray(boss, bubblegum, arena, gridUid, grid, target, rage, now);
            warped = TryBloodWarp(boss, bubblegum, arena, gridUid, grid, target);
            if (warped)
                MarkPressure(bubblegum, now, "blood-warp", target);
        }

        var shouldSummon = forcePressure || !_random.Prob(Math.Clamp((88f - rage) / 100f, 0f, 1f));
        if (shouldSummon &&
            TrySummonSlaughterlings(boss, bubblegum, arena, gridUid, grid, now, out var summonedFullWave) &&
            summonedFullWave)
        {
            MarkPressure(bubblegum, now, "summon", target);
            bubblegum.BusyUntil = now + TimeSpan.FromSeconds(0.6);
            bubblegum.NextAttack = now + GetScaledCooldown(bubblegum.RangedCooldown, rage);
            return;
        }

        if (belowHalf)
        {
            if (_random.Prob(0.70f) || warped)
                StartCharge(boss, bubblegum, arena, gridUid, grid, target, bubblegum.TripleChargeSteps, 2, now);
            else
            {
                TryBloodWarp(boss, bubblegum, arena, gridUid, grid, target);
                StartCharge(boss, bubblegum, arena, gridUid, grid, target, bubblegum.ChargeMaxSteps, 0, now);
            }
        }
        else
        {
            StartCharge(boss, bubblegum, arena, gridUid, grid, target, bubblegum.ChargeMaxSteps, 0, now);
        }

        MarkPressure(bubblegum, now, belowHalf ? "triple-charge" : "charge", target);
        bubblegum.NextAttack = now + GetScaledCooldown(bubblegum.RangedCooldown, rage);
    }

    private bool TryQueueBloodAttack(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        TimeSpan now)
    {
        _bloodTargets.Clear();
        foreach (var participant in _participants)
        {
            var tile = GetEntityTile(participant, gridUid, grid);
            if (tile != null && HasBloodPoolWithin(bubblegum, gridUid, tile.Value, 1))
                _bloodTargets.Add(participant);
        }

        if (_bloodTargets.Count == 0)
            return false;

        var attacks = Math.Min(2, _bloodTargets.Count);
        var rightHand = _random.Prob(0.5f);
        var latestAttack = now;
        for (var i = 0; i < attacks; i++)
        {
            var target = _random.PickAndTake(_bloodTargets);
            var tile = GetEntityTile(target, gridUid, grid);
            if (tile == null)
                continue;

            var grabChance = IsBelowHalfHealth(boss)
                ? bubblegum.BloodGrabChanceBelowHalf
                : bubblegum.BloodGrabChance;
            var grab = (TryComp<MobStateComponent>(target, out var targetMobState) &&
                targetMobState.CurrentState != MobState.Alive) || _random.Prob(Math.Clamp(grabChance, 0f, 1f));
            QueueHandAttack(bubblegum, gridUid, grid, tile.Value, now, grab, rightHand);
            MarkTargetPressure(bubblegum, target, now);
            latestAttack = now + (grab ? bubblegum.BloodGrabDelay : bubblegum.BloodSmackDelay);
            rightHand = !rightHand;
        }

        bubblegum.BusyUntil = latestAttack + TimeSpan.FromSeconds(0.25);
        return true;
    }

    private void QueueHandAttack(
        LavalandBubblegumComponent bubblegum,
        EntityUid gridUid,
        MapGridComponent grid,
        Vector2i tile,
        TimeSpan now,
        bool grab,
        bool rightHand)
    {
        if (grab)
        {
            SpawnAnchored(rightHand ? bubblegum.RightPawPrototype : bubblegum.LeftPawPrototype, gridUid, grid, tile);
            SpawnAnchored(rightHand ? bubblegum.RightThumbPrototype : bubblegum.LeftThumbPrototype, gridUid, grid, tile);
        }
        else
        {
            SpawnAnchored(rightHand ? bubblegum.RightSmackPrototype : bubblegum.LeftSmackPrototype, gridUid, grid, tile);
        }

        bubblegum.PendingHandAttacks.Add(new LavalandBubblegumPendingHandAttack
        {
            Grid = gridUid,
            Tile = tile,
            AttackAt = now + (grab ? bubblegum.BloodGrabDelay : bubblegum.BloodSmackDelay),
            Grab = grab,
            RightHand = rightHand,
        });
    }

    private void ProcessPendingHandAttacks(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        TimeSpan now)
    {
        for (var i = bubblegum.PendingHandAttacks.Count - 1; i >= 0; i--)
        {
            var pending = bubblegum.PendingHandAttacks[i];
            if (pending.AttackAt > now)
                continue;

            if (pending.Grid == gridUid)
                DamageHandTile(boss, bubblegum, arena, gridUid, grid, pending.Tile, pending.Grab);

            bubblegum.PendingHandAttacks.RemoveAt(i);
        }
    }

    private void DamageHandTile(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        Vector2i tile,
        bool grab)
    {
        var bossTile = GetEntityTile(boss, gridUid, grid);
        var hit = false;
        var query = EntityQueryEnumerator<DamageableComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var damageable, out var mobState, out var xform))
        {
            if (uid == boss ||
                bubblegum.Slaughterlings.Contains(uid) ||
                mobState.CurrentState == MobState.Dead ||
                xform.GridUid != gridUid ||
                _map.LocalToTile(gridUid, grid, xform.Coordinates) != tile)
            {
                continue;
            }

            _damageable.TryChangeDamage((uid, damageable), grab ? bubblegum.GrabDamage : bubblegum.SmackDamage, origin: boss);
            hit = true;

            if (!grab || bossTile == null)
                continue;

            var direction = StepTowards(bossTile.Value, tile) - bossTile.Value;
            if (direction == Vector2i.Zero)
                direction = _random.Pick(Cardinals);

            var destination = ClampToInnerArena(arena, bossTile.Value + direction);
            _audio.PlayPvs(bubblegum.EnterBloodSound, uid, AudioParams.Default.WithVolume(-3f));
            _transform.SetCoordinates(uid, _map.GridTileToLocal(gridUid, grid, destination));
            _audio.PlayPvs(bubblegum.ExitBloodSound, uid, AudioParams.Default.WithVolume(-3f));
        }

        if (hit)
            _audio.PlayPvs(bubblegum.AttackSound, _map.GridTileToLocal(gridUid, grid, tile), AudioParams.Default.WithVolume(-2f));
    }

    private void QueueBloodSpray(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        EntityUid target,
        float rage,
        TimeSpan now)
    {
        var bossTile = GetEntityTile(boss, gridUid, grid);
        var targetTile = GetEntityTile(target, gridUid, grid);
        if (bossTile == null || targetTile == null)
            return;

        var direction = StepTowards(bossTile.Value, targetTile.Value) - bossTile.Value;
        if (direction == Vector2i.Zero)
            direction = _random.Pick(Cardinals);

        TrySpawnBloodPool(bubblegum, arena, gridUid, grid, bossTile.Value);
        var range = Math.Max(1, bubblegum.BloodSprayBaseRange + (int) MathF.Round(rage * bubblegum.BloodSprayRageRangeMultiplier));
        for (var step = 1; step <= range; step++)
        {
            if (bubblegum.PendingBloodTiles.Count >= Math.Max(0, bubblegum.MaxPendingBloodTiles))
                break;

            var tile = bossTile.Value + direction * step;
            if (!IsInsideInnerArena(arena, tile))
                break;

            if (HasBloodPoolAt(bubblegum, gridUid, tile) || HasPendingBloodTile(bubblegum, gridUid, tile))
                continue;

            bubblegum.PendingBloodTiles.Add(new LavalandBubblegumPendingBloodTile
            {
                Grid = gridUid,
                Tile = tile,
                SpawnAt = now + TimeSpan.FromSeconds(bubblegum.BloodSprayStepDelay.TotalSeconds * step),
            });
        }

        _audio.PlayPvs(bubblegum.SplatSound, boss, AudioParams.Default.WithVolume(-2f));
    }

    private void ProcessPendingBloodTiles(
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        TimeSpan now)
    {
        var playedSound = false;
        for (var i = bubblegum.PendingBloodTiles.Count - 1; i >= 0; i--)
        {
            var pending = bubblegum.PendingBloodTiles[i];
            if (pending.SpawnAt > now)
                continue;

            if (pending.Grid == gridUid && IsInsideInnerArena(arena, pending.Tile))
            {
                TrySpawnBloodPool(bubblegum, arena, gridUid, grid, pending.Tile);
                if (_random.Prob(0.65f))
                    SpawnAnchored(bubblegum.BloodSplatterPrototype, gridUid, grid, pending.Tile);

                if (!playedSound)
                {
                    _audio.PlayPvs(bubblegum.SplatSound, _map.GridTileToLocal(gridUid, grid, pending.Tile), AudioParams.Default.WithVolume(-5f));
                    playedSound = true;
                }
            }

            bubblegum.PendingBloodTiles.RemoveAt(i);
        }
    }

    private bool QueueBloodPressureAtTarget(
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        Vector2i targetTile,
        TimeSpan now,
        bool urgent)
    {
        if (!IsInsideInnerArena(arena, targetTile))
            return false;

        var queued = TrySpawnBloodPool(bubblegum, arena, gridUid, grid, targetTile);
        var maxAdjacent = urgent ? 4 : 2;
        var addedAdjacent = 0;

        foreach (var direction in Cardinals)
        {
            if (addedAdjacent >= maxAdjacent ||
                bubblegum.PendingBloodTiles.Count >= Math.Max(0, bubblegum.MaxPendingBloodTiles))
            {
                break;
            }

            var tile = targetTile + direction;
            if (!IsInsideInnerArena(arena, tile) ||
                HasBloodPoolAt(bubblegum, gridUid, tile) ||
                HasPendingBloodTile(bubblegum, gridUid, tile))
            {
                continue;
            }

            bubblegum.PendingBloodTiles.Add(new LavalandBubblegumPendingBloodTile
            {
                Grid = gridUid,
                Tile = tile,
                SpawnAt = now + TimeSpan.FromSeconds(0.06 * (addedAdjacent + 1)),
            });
            addedAdjacent++;
            queued = true;
        }

        if (bubblegum.PendingHandAttacks.Count < 12)
        {
            var grabChance = urgent ? bubblegum.BloodGrabChanceBelowHalf : bubblegum.BloodGrabChance;
            QueueHandAttack(
                bubblegum,
                gridUid,
                grid,
                targetTile,
                now + (urgent ? TimeSpan.Zero : TimeSpan.FromSeconds(0.12)),
                _random.Prob(Math.Clamp(grabChance, 0f, 1f)),
                _random.Prob(0.5f));
            queued = true;
        }

        if (queued)
            _audio.PlayPvs(bubblegum.SplatSound, _map.GridTileToLocal(gridUid, grid, targetTile), AudioParams.Default.WithVolume(-3f));

        return queued;
    }

    private bool TryBloodWarp(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        EntityUid target)
    {
        var bossTile = GetEntityTile(boss, gridUid, grid);
        var targetTile = GetEntityTile(target, gridUid, grid);
        if (bossTile == null ||
            targetTile == null ||
            ChebyshevDistance(bossTile.Value, targetTile.Value) <= 1)
        {
            return false;
        }

        GetPoolsAround(bubblegum, gridUid, bossTile.Value, 1, _poolTiles);
        if (_poolTiles.Count == 0)
            return false;

        GetPoolsAround(bubblegum, gridUid, targetTile.Value, 2, _poolTiles);
        for (var i = _poolTiles.Count - 1; i >= 0; i--)
        {
            if (ChebyshevDistance(_poolTiles[i], targetTile.Value) <= 1)
                _poolTiles.RemoveAt(i);
        }

        if (_poolTiles.Count == 0)
            return false;

        var destination = _random.Pick(_poolTiles);
        destination = ClampToInnerArena(arena, destination);

        _audio.PlayPvs(bubblegum.EnterBloodSound, boss, AudioParams.Default.WithVolume(-2f));
        SpawnAnchored(bubblegum.BloodSplatterPrototype, gridUid, grid, bossTile.Value);
        _transform.SetCoordinates(boss, _map.GridTileToLocal(gridUid, grid, destination));
        SpawnAnchored(bubblegum.BloodSplatterPrototype, gridUid, grid, destination);
        _audio.PlayPvs(bubblegum.ExitBloodSound, boss, AudioParams.Default.WithVolume(-2f));
        return true;
    }

    private bool TrySummonSlaughterlings(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        TimeSpan now,
        out bool summonedFullWave)
    {
        summonedFullWave = false;
        if (now < bubblegum.NextSummon)
            return false;

        PruneTracked(bubblegum);
        var maxActive = Math.Max(0, bubblegum.MaxActiveSlaughterlings);
        if (maxActive == 0)
            return false;

        var active = bubblegum.Slaughterlings.Count;
        if (active >= maxActive)
            return false;

        var bossTile = GetEntityTile(boss, gridUid, grid);
        if (bossTile == null)
            return false;

        GetPoolsAround(bubblegum, gridUid, bossTile.Value, 1, _poolTiles);
        _random.Shuffle(_poolTiles);

        var limit = Math.Min(
            Math.Max(0, bubblegum.MaxSummonsPerCast),
            Math.Max(0, maxActive - active));
        if (limit <= 0)
            return false;

        var spawned = 0;
        foreach (var tile in _poolTiles)
        {
            if (spawned >= limit)
                break;

            if (!IsInsideInnerArena(arena, tile))
                continue;

            var summon = Spawn(bubblegum.SlaughterlingPrototype, _map.GridTileToLocal(gridUid, grid, tile));
            bubblegum.Slaughterlings.Add(summon);
            spawned++;
        }

        if (spawned <= 0)
            return false;

        summonedFullWave = spawned >= Math.Max(1, bubblegum.MaxSummonsPerCast);
        bubblegum.NextSummon = now + bubblegum.SummonCooldown;
        _audio.PlayPvs(bubblegum.SplatSound, _map.GridTileToLocal(gridUid, grid, bossTile.Value), AudioParams.Default.WithVolume(-1f));
        return true;
    }

    private void StartCharge(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        EntityUid target,
        int steps,
        int extraCharges,
        TimeSpan now)
    {
        var bossTile = GetEntityTile(boss, gridUid, grid);
        var targetTile = GetEntityTile(target, gridUid, grid);
        if (bossTile == null || targetTile == null)
            return;

        var destination = ClampToInnerArena(arena, targetTile.Value);
        SpawnAnchored(bubblegum.LandingPrototype, gridUid, grid, destination);

        bubblegum.Charging = true;
        bubblegum.ChargeTargetTile = destination;
        bubblegum.ChargeRemainingSteps = Math.Max(1, Math.Min(Math.Max(1, steps), Math.Max(1, ChebyshevDistance(bossTile.Value, destination))));
        bubblegum.NextChargeStep = now + bubblegum.ChargeWindup;
        bubblegum.PendingCharges = Math.Max(0, extraCharges);
        bubblegum.PendingChargeSteps = Math.Max(1, steps);
        bubblegum.NextQueuedCharge = TimeSpan.Zero;
        bubblegum.ChargeHitEntities.Clear();
        bubblegum.BusyUntil = now + bubblegum.ChargeWindup + TimeSpan.FromSeconds(bubblegum.ChargeStepDelay.TotalSeconds * bubblegum.ChargeRemainingSteps) + bubblegum.ChargeRecover;

        _movement.RefreshMovementSpeedModifiers(boss);
    }

    private bool ProcessCharge(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        TimeSpan now)
    {
        if (!bubblegum.Charging)
            return false;

        if (now < bubblegum.NextChargeStep)
            return true;

        var currentTile = GetEntityTile(boss, gridUid, grid);
        if (currentTile == null ||
            currentTile.Value == bubblegum.ChargeTargetTile ||
            bubblegum.ChargeRemainingSteps <= 0)
        {
            FinishCharge(boss, bubblegum, gridUid, grid, now);
            return true;
        }

        var nextTile = StepTowards(currentTile.Value, bubblegum.ChargeTargetTile);
        if (!IsInsideInnerArena(arena, nextTile))
        {
            FinishCharge(boss, bubblegum, gridUid, grid, now);
            return true;
        }

        TrySpawnBloodPool(bubblegum, arena, gridUid, grid, currentTile.Value);
        TrySpawnBloodPool(bubblegum, arena, gridUid, grid, nextTile);
        var chargeDirection = nextTile - currentTile.Value;
        _transform.SetCoordinates(boss, _map.GridTileToLocal(gridUid, grid, nextTile));

        var hit = DamageChargeTile(boss, bubblegum, gridUid, grid, nextTile, chargeDirection);
        bubblegum.ChargeRemainingSteps--;
        bubblegum.NextChargeStep = now + bubblegum.ChargeStepDelay;

        if (hit ||
            nextTile == bubblegum.ChargeTargetTile ||
            bubblegum.ChargeRemainingSteps <= 0)
        {
            FinishCharge(boss, bubblegum, gridUid, grid, bubblegum.NextChargeStep);
        }

        return true;
    }

    private bool ProcessQueuedCharge(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        TimeSpan now)
    {
        if (bubblegum.PendingCharges <= 0)
            return false;

        if (now < bubblegum.NextQueuedCharge)
            return true;

        var target = PickTarget(bubblegum, boss, gridUid, grid, now);
        if (target == null)
        {
            bubblegum.PendingCharges = 0;
            return false;
        }

        var remaining = Math.Max(0, bubblegum.PendingCharges - 1);
        StartCharge(boss, bubblegum, arena, gridUid, grid, target.Value, bubblegum.PendingChargeSteps, remaining, now);
        MarkTargetPressure(bubblegum, target.Value, now);
        return true;
    }

    private bool DamageChargeTile(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        EntityUid gridUid,
        MapGridComponent grid,
        Vector2i tile,
        Vector2i chargeDirection)
    {
        var hit = false;
        var query = EntityQueryEnumerator<DamageableComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var damageable, out var mobState, out var xform))
        {
            if (uid == boss ||
                bubblegum.Slaughterlings.Contains(uid) ||
                bubblegum.ChargeHitEntities.Contains(uid) ||
                mobState.CurrentState == MobState.Dead ||
                xform.GridUid != gridUid ||
                _map.LocalToTile(gridUid, grid, xform.Coordinates) != tile)
            {
                continue;
            }

            _damageable.TryChangeDamage((uid, damageable), bubblegum.ChargeDamage, origin: boss);
            bubblegum.ChargeHitEntities.Add(uid);
            hit = true;

            var direction = new Vector2(chargeDirection.X, chargeDirection.Y);
            if (direction.LengthSquared() < 0.01f)
                direction = _random.NextVector2();

            _throwing.TryThrow(uid, direction.Normalized() * 2.5f, bubblegum.ChargeThrowSpeed, boss, playSound: false, doSpin: false);
        }

        if (hit)
            _audio.PlayPvs(bubblegum.ImpactSound, _map.GridTileToLocal(gridUid, grid, tile), AudioParams.Default.WithVolume(0f));

        return hit;
    }

    private void FinishCharge(
        EntityUid boss,
        LavalandBubblegumComponent bubblegum,
        EntityUid gridUid,
        MapGridComponent grid,
        TimeSpan now)
    {
        var tile = GetEntityTile(boss, gridUid, grid) ?? Vector2i.Zero;
        _audio.PlayPvs(bubblegum.ImpactSound, _map.GridTileToLocal(gridUid, grid, tile), AudioParams.Default.WithVolume(-2f));

        bubblegum.Charging = false;
        bubblegum.ChargeHitEntities.Clear();
        _movement.RefreshMovementSpeedModifiers(boss);

        if (bubblegum.PendingCharges > 0)
        {
            bubblegum.NextQueuedCharge = now + bubblegum.ChainedChargeDelay;
            bubblegum.BusyUntil = bubblegum.NextQueuedCharge + bubblegum.ChargeWindup;
        }
        else
        {
            bubblegum.NextQueuedCharge = TimeSpan.Zero;
            bubblegum.BusyUntil = now + bubblegum.ChargeRecover;
        }
    }

    private bool TrySpawnBloodPool(
        LavalandBubblegumComponent bubblegum,
        LavalandBossArenaComponent arena,
        EntityUid gridUid,
        MapGridComponent grid,
        Vector2i tile)
    {
        if (!IsInsideInnerArena(arena, tile) ||
            HasBloodPoolAt(bubblegum, gridUid, tile) ||
            !_prototype.HasIndex<EntityPrototype>(bubblegum.BloodPoolPrototype))
        {
            return false;
        }

        PruneTracked(bubblegum);
        if (bubblegum.BloodPools.Count >= Math.Max(1, bubblegum.MaxBloodPools))
        {
            var oldest = bubblegum.BloodPools[0];
            bubblegum.BloodPools.RemoveAt(0);
            if (Exists(oldest))
                QueueDel(oldest);
        }

        var pool = Spawn(bubblegum.BloodPoolPrototype, _map.GridTileToLocal(gridUid, grid, tile));
        if (TryComp(pool, out TransformComponent? xform) && !xform.Anchored)
            _transform.AnchorEntity((pool, xform), (gridUid, grid), tile);

        var poolComponent = EnsureComp<LavalandBubblegumBloodPoolComponent>(pool);
        poolComponent.Grid = gridUid;
        poolComponent.Tile = tile;

        bubblegum.BloodPools.Add(pool);
        return true;
    }

    private void SpawnAnchored(string prototype, EntityUid gridUid, MapGridComponent grid, Vector2i index)
    {
        if (string.IsNullOrWhiteSpace(prototype) ||
            !_prototype.HasIndex<EntityPrototype>(prototype))
        {
            return;
        }

        var uid = Spawn(prototype, _map.GridTileToLocal(gridUid, grid, index));
        if (!TryComp(uid, out TransformComponent? xform) || xform.Anchored)
            return;

        _transform.AnchorEntity((uid, xform), (gridUid, grid), index);
    }

    private void GetPoolsAround(LavalandBubblegumComponent bubblegum, EntityUid gridUid, Vector2i center, int range, List<Vector2i> output)
    {
        output.Clear();
        foreach (var pool in bubblegum.BloodPools)
        {
            if (!TryComp<LavalandBubblegumBloodPoolComponent>(pool, out var poolComponent) ||
                poolComponent.Grid != gridUid ||
                ChebyshevDistance(poolComponent.Tile, center) > range ||
                output.Contains(poolComponent.Tile))
            {
                continue;
            }

            output.Add(poolComponent.Tile);
        }
    }

    private bool HasBloodPoolAt(LavalandBubblegumComponent bubblegum, EntityUid gridUid, Vector2i tile)
    {
        foreach (var pool in bubblegum.BloodPools)
        {
            if (TryComp<LavalandBubblegumBloodPoolComponent>(pool, out var poolComponent) &&
                poolComponent.Grid == gridUid &&
                poolComponent.Tile == tile)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasBloodPoolWithin(LavalandBubblegumComponent bubblegum, EntityUid gridUid, Vector2i tile, int range)
    {
        foreach (var pool in bubblegum.BloodPools)
        {
            if (TryComp<LavalandBubblegumBloodPoolComponent>(pool, out var poolComponent) &&
                poolComponent.Grid == gridUid &&
                ChebyshevDistance(poolComponent.Tile, tile) <= range)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasPendingBloodTile(LavalandBubblegumComponent bubblegum, EntityUid gridUid, Vector2i tile)
    {
        foreach (var pending in bubblegum.PendingBloodTiles)
        {
            if (pending.Grid == gridUid && pending.Tile == tile)
                return true;
        }

        return false;
    }

    private int CollectParticipants(LavalandBossArenaComponent arena)
    {
        _participants.Clear();

        foreach (var userId in arena.Participants)
        {
            if (!_players.TryGetSessionById(userId, out var session) ||
                session.AttachedEntity is not { Valid: true } attached ||
                !Exists(attached) ||
                IsDead(attached) ||
                !TryComp(attached, out TransformComponent? xform) ||
                xform.GridUid != arena.Grid)
            {
                continue;
            }

            _participants.Add(attached);
        }

        return _participants.Count;
    }

    private EntityUid? PickTarget(
        LavalandBubblegumComponent bubblegum,
        EntityUid boss,
        EntityUid gridUid,
        MapGridComponent grid,
        TimeSpan now)
    {
        if (_participants.Count == 0)
            return null;

        PruneTargetMemory(bubblegum);

        if (_participants.Count == 1)
        {
            SetPrimaryTarget(bubblegum, _participants[0], now);
            return _participants[0];
        }

        if (bubblegum.CurrentPrimaryTarget is { Valid: true } current &&
            _participants.Contains(current) &&
            now - bubblegum.LastTargetSwitchAt < bubblegum.TargetSwitchCooldown)
        {
            return current;
        }

        var bossTile = boss.Valid ? GetEntityTile(boss, gridUid, grid) : null;
        EntityUid? best = null;
        var bestScore = float.MinValue;
        foreach (var participant in _participants)
        {
            var tile = GetEntityTile(participant, gridUid, grid);
            if (tile == null)
                continue;

            var score = ScoreTarget(bubblegum, participant, bossTile, tile.Value, now, true);
            if (score <= bestScore)
                continue;

            best = participant;
            bestScore = score;
        }

        if (best == null)
            best = _random.Pick(_participants);

        SetPrimaryTarget(bubblegum, best.Value, now);
        return best;
    }

    private EntityUid? PickSecondaryTarget(
        LavalandBubblegumComponent bubblegum,
        EntityUid excluded,
        EntityUid gridUid,
        MapGridComponent grid,
        TimeSpan now)
    {
        if (_participants.Count <= 1)
            return null;

        PruneTargetMemory(bubblegum);

        EntityUid? best = null;
        var bestScore = float.MinValue;
        foreach (var participant in _participants)
        {
            if (participant == excluded)
                continue;

            var tile = GetEntityTile(participant, gridUid, grid);
            if (tile == null)
                continue;

            var score = ScoreTarget(bubblegum, participant, null, tile.Value, now, false);
            if (score <= bestScore)
                continue;

            best = participant;
            bestScore = score;
        }

        return best;
    }

    private float ScoreTarget(
        LavalandBubblegumComponent bubblegum,
        EntityUid target,
        Vector2i? bossTile,
        Vector2i targetTile,
        TimeSpan now,
        bool applyCurrentPenalty)
    {
        var safeSeconds = bubblegum.TargetPressureMemory.TotalSeconds;
        if (bubblegum.LastPressureByTarget.TryGetValue(target, out var lastPressure))
            safeSeconds = Math.Clamp((now - lastPressure).TotalSeconds, 0, bubblegum.TargetPressureMemory.TotalSeconds);

        var distancePenalty = bossTile == null
            ? 0f
            : ChebyshevDistance(bossTile.Value, targetTile) * 0.2f;
        var score = (float) safeSeconds - distancePenalty + _random.NextFloat(0f, 1.5f);

        if (applyCurrentPenalty &&
            bubblegum.CurrentPrimaryTarget == target &&
            now - bubblegum.LastTargetSwitchAt >= bubblegum.TargetSwitchCooldown)
        {
            score -= 8f;
        }

        return score;
    }

    private void PruneTargetMemory(LavalandBubblegumComponent bubblegum)
    {
        if (bubblegum.CurrentPrimaryTarget is { Valid: true } current &&
            !_participants.Contains(current))
        {
            bubblegum.CurrentPrimaryTarget = null;
        }

        if (bubblegum.LastPressureByTarget.Count == 0)
            return;

        foreach (var target in new List<EntityUid>(bubblegum.LastPressureByTarget.Keys))
        {
            if (!_participants.Contains(target))
                bubblegum.LastPressureByTarget.Remove(target);
        }
    }

    private static void SetPrimaryTarget(LavalandBubblegumComponent bubblegum, EntityUid target, TimeSpan now)
    {
        if (bubblegum.CurrentPrimaryTarget == target)
            return;

        bubblegum.CurrentPrimaryTarget = target;
        bubblegum.LastTargetSwitchAt = now;
    }

    private void PruneTracked(LavalandBubblegumComponent bubblegum)
    {
        for (var i = bubblegum.BloodPools.Count - 1; i >= 0; i--)
        {
            if (!Exists(bubblegum.BloodPools[i]))
                bubblegum.BloodPools.RemoveAt(i);
        }

        for (var i = bubblegum.Slaughterlings.Count - 1; i >= 0; i--)
        {
            var summon = bubblegum.Slaughterlings[i];
            if (!Exists(summon) || IsDead(summon))
                bubblegum.Slaughterlings.RemoveAt(i);
        }
    }

    private void ClearRuntimeState(EntityUid uid, LavalandBubblegumComponent bubblegum, bool refreshMovement)
    {
        bubblegum.PendingBloodTiles.Clear();
        bubblegum.PendingHandAttacks.Clear();
        bubblegum.Charging = false;
        bubblegum.PendingCharges = 0;
        bubblegum.NextQueuedCharge = TimeSpan.Zero;
        bubblegum.ChargeHitEntities.Clear();
        bubblegum.CurrentPrimaryTarget = null;
        bubblegum.LastTargetSwitchAt = TimeSpan.Zero;
        bubblegum.LastPressureByTarget.Clear();

        foreach (var pool in bubblegum.BloodPools)
        {
            if (Exists(pool))
                QueueDel(pool);
        }

        bubblegum.BloodPools.Clear();

        foreach (var summon in bubblegum.Slaughterlings)
        {
            if (Exists(summon))
                QueueDel(summon);
        }

        bubblegum.Slaughterlings.Clear();

        if (refreshMovement && Exists(uid))
            _movement.RefreshMovementSpeedModifiers(uid);
    }

    private static bool NeedsPressure(LavalandBubblegumComponent bubblegum, TimeSpan now)
    {
        return bubblegum.LastPressureAt == TimeSpan.Zero ||
               now - bubblegum.LastPressureAt >= bubblegum.ForcePressureAfter;
    }

    private static void MarkPressure(LavalandBubblegumComponent bubblegum, TimeSpan now, string attackKind, EntityUid target)
    {
        bubblegum.LastPressureAt = now;
        bubblegum.LastAttackKind = attackKind;
        MarkTargetPressure(bubblegum, target, now);
    }

    private static void MarkTargetPressure(LavalandBubblegumComponent bubblegum, EntityUid target, TimeSpan now)
    {
        if (!target.Valid)
            return;

        bubblegum.LastPressureByTarget[target] = now;
    }

    private float CalculateRage(EntityUid boss)
    {
        if (!TryComp<DamageableComponent>(boss, out var damageable))
            return 0f;

        return Math.Clamp(Math.Max(0f, damageable.TotalDamage.Float()) / 60f, 0f, 20f);
    }

    private bool IsBelowHalfHealth(EntityUid boss)
    {
        if (!TryComp<LavalandBossComponent>(boss, out var bossComp) ||
            !TryComp<DamageableComponent>(boss, out var damageable))
        {
            return false;
        }

        return damageable.TotalDamage.Float() >= bossComp.MaxHealth * 0.5f;
    }

    private static TimeSpan GetScaledCooldown(TimeSpan baseCooldown, float rage)
    {
        return TimeSpan.FromSeconds(Math.Max(2.25, baseCooldown.TotalSeconds - rage * 0.04));
    }

    private static TimeSpan GetBloodReactionWindow(LavalandBubblegumComponent bubblegum)
    {
        return TimeSpan.FromSeconds(Math.Max(bubblegum.BloodSmackDelay.TotalSeconds, bubblegum.BloodGrabDelay.TotalSeconds)) +
               bubblegum.BloodHandRecover;
    }

    private Vector2i? GetEntityTile(EntityUid uid, EntityUid gridUid, MapGridComponent grid)
    {
        if (!uid.Valid ||
            !TryComp(uid, out TransformComponent? xform) ||
            xform.GridUid != gridUid)
        {
            return null;
        }

        return _map.LocalToTile(gridUid, grid, xform.Coordinates);
    }

    private bool IsDead(EntityUid uid)
    {
        return TryComp(uid, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead;
    }

    private static Vector2i StepTowards(Vector2i from, Vector2i to)
    {
        return new Vector2i(
            from.X + Math.Sign(to.X - from.X),
            from.Y + Math.Sign(to.Y - from.Y));
    }

    private static int ChebyshevDistance(Vector2i a, Vector2i b)
    {
        return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }

    private static bool IsInsideInnerArena(LavalandBossArenaComponent arena, Vector2i tile)
    {
        var (minX, maxX, minY, maxY) = GetInnerBounds(arena);
        return tile.X >= minX && tile.X <= maxX && tile.Y >= minY && tile.Y <= maxY;
    }

    private static Vector2i ClampToInnerArena(LavalandBossArenaComponent arena, Vector2i tile)
    {
        var (minX, maxX, minY, maxY) = GetInnerBounds(arena);
        return new Vector2i(
            Math.Clamp(tile.X, minX, maxX),
            Math.Clamp(tile.Y, minY, maxY));
    }

    private static (int MinX, int MaxX, int MinY, int MaxY) GetInnerBounds(LavalandBossArenaComponent arena)
    {
        var halfWidth = arena.Width / 2;
        var halfHeight = arena.Height / 2;
        return (-halfWidth + 1, halfWidth - 1, -halfHeight + 1, halfHeight - 1);
    }
}
