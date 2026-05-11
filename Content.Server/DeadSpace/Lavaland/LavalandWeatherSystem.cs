using Content.Server.DeadSpace.Lavaland.Components;
using Content.Server.Chat.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DeadSpace.Lavaland;
using Content.Shared.DeadSpace.Lavaland.Components;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weather;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Lavaland;

public sealed class LavalandWeatherSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private EntityQuery<LavalandWeatherImmuneComponent> _immuneQuery;
    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();

        _immuneQuery = GetEntityQuery<LavalandWeatherImmuneComponent>();
        _gridQuery = GetEntityQuery<MapGridComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<LavalandMapComponent>();
        while (query.MoveNext(out var uid, out var lavaland))
        {
            if (!_prototype.TryIndex(lavaland.Planet, out var planet))
                continue;

            if (!planet.AshStormEnabled)
            {
                ResetWeatherState(uid, lavaland);
                continue;
            }

            if (!lavaland.WeatherInitialized)
                ScheduleNextStorm(lavaland, planet, curTime);

            switch (lavaland.WeatherStage)
            {
                case LavalandWeatherStage.Calm:
                    if (curTime >= lavaland.NextWeatherTime)
                        StartWarning(uid, lavaland, planet, curTime);
                    break;

                case LavalandWeatherStage.Warning:
                    if (curTime >= lavaland.WeatherStageEndTime)
                        StartStorm(uid, lavaland, planet, curTime);
                    break;

                case LavalandWeatherStage.Active:
                    if (curTime >= lavaland.WeatherStageEndTime)
                    {
                        EndStorm(uid, lavaland, planet, curTime);
                        break;
                    }

                    if (curTime >= lavaland.NextWeatherDamageTime)
                        ApplyStormDamage(uid, lavaland, planet, curTime);
                    break;
            }
        }
    }

    private void StartWarning(
        EntityUid mapUid,
        LavalandMapComponent lavaland,
        LavalandPlanetPrototype planet,
        TimeSpan curTime)
    {
        var duration = ClampDuration(planet.AshStormWarningDuration);

        if (duration == TimeSpan.Zero)
        {
            StartStorm(mapUid, lavaland, planet, curTime);
            return;
        }

        if (_prototype.Resolve(planet.AshStormWarningWeather, out var weather))
            _weather.SetWeather(Transform(mapUid).MapID, weather, curTime + duration);

        WarnPlayers(mapUid);

        lavaland.WeatherStage = LavalandWeatherStage.Warning;
        lavaland.WeatherStageEndTime = curTime + duration;
    }

    private void StartStorm(
        EntityUid mapUid,
        LavalandMapComponent lavaland,
        LavalandPlanetPrototype planet,
        TimeSpan curTime)
    {
        var duration = PickDuration(planet.AshStormDurationMin, planet.AshStormDurationMax);

        if (_prototype.Resolve(planet.AshStormWeather, out var weather))
            _weather.SetWeather(Transform(mapUid).MapID, weather, curTime + duration);

        lavaland.WeatherStage = LavalandWeatherStage.Active;
        lavaland.WeatherStageEndTime = curTime + duration;
        lavaland.NextWeatherDamageTime = curTime + ClampDamageInterval(planet);
    }

    private void EndStorm(
        EntityUid mapUid,
        LavalandMapComponent lavaland,
        LavalandPlanetPrototype planet,
        TimeSpan curTime)
    {
        lavaland.WeatherStage = LavalandWeatherStage.Calm;
        lavaland.WeatherStageEndTime = TimeSpan.Zero;
        lavaland.NextWeatherDamageTime = TimeSpan.Zero;
        ScheduleNextStorm(lavaland, planet, curTime);
    }

    private void WarnPlayers(EntityUid mapUid)
    {
        var filter = Filter.Empty().AddInMap(Transform(mapUid).MapID);
        _chat.DispatchFilteredAnnouncement(
            filter,
            Loc.GetString("lavaland-ash-storm-warning"),
            sender: Loc.GetString("lavaland-ash-storm-sender"),
            playSound: false,
            colorOverride: Color.OrangeRed);
    }

    private void ResetWeatherState(EntityUid mapUid, LavalandMapComponent lavaland)
    {
        if (lavaland.WeatherStage == LavalandWeatherStage.Calm && !lavaland.WeatherInitialized)
            return;

        lavaland.WeatherStage = LavalandWeatherStage.Calm;
        lavaland.WeatherInitialized = false;
        lavaland.NextWeatherTime = TimeSpan.Zero;
        lavaland.WeatherStageEndTime = TimeSpan.Zero;
        lavaland.NextWeatherDamageTime = TimeSpan.Zero;
        _weather.SetWeather(Transform(mapUid).MapID, null, null);
    }

    private void ScheduleNextStorm(
        LavalandMapComponent lavaland,
        LavalandPlanetPrototype planet,
        TimeSpan curTime)
    {
        lavaland.WeatherInitialized = true;
        lavaland.NextWeatherTime = curTime + PickDuration(planet.AshStormCooldownMin, planet.AshStormCooldownMax);
    }

    private void ApplyStormDamage(
        EntityUid mapUid,
        LavalandMapComponent lavaland,
        LavalandPlanetPrototype planet,
        TimeSpan curTime)
    {
        lavaland.NextWeatherDamageTime = curTime + ClampDamageInterval(planet);

        if (planet.AshStormDamage.Empty)
            return;

        var query = EntityQueryEnumerator<DamageableComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var damageable, out var mobState, out var xform))
        {
            if (xform.MapUid != mapUid ||
                xform.GridUid is not { } gridUid ||
                !_gridQuery.TryGetComponent(gridUid, out var grid) ||
                !_map.TryGetTileRef(gridUid, grid, xform.Coordinates, out var tile) ||
                tile.Tile.IsEmpty)
            {
                continue;
            }

            if (_mobState.IsDead(uid, mobState) ||
                IsWeatherImmune(uid, xform) ||
                !_weather.CanWeatherAffect(gridUid, grid, tile))
            {
                continue;
            }

            var damage = new DamageSpecifier(planet.AshStormDamage);
            _damageable.TryChangeDamage((uid, damageable), damage, interruptsDoAfters: false, origin: mapUid);
        }
    }

    private bool IsWeatherImmune(EntityUid uid, TransformComponent xform)
    {
        while (true)
        {
            if (_immuneQuery.HasComponent(uid) || HasCompleteEquippedWeatherProtection(uid))
                return true;

            var parent = xform.ParentUid;
            if (!parent.IsValid() || parent == uid || !_xformQuery.TryGetComponent(parent, out var parentXform))
                return false;

            uid = parent;
            xform = parentXform;
        }
    }

    private bool HasCompleteEquippedWeatherProtection(EntityUid uid)
    {
        var requiredSlots = SlotFlags.OUTERCLOTHING | SlotFlags.HEAD;
        var protectedSlots = SlotFlags.NONE;
        var enumerator = _inventory.GetSlotEnumerator(uid, requiredSlots);

        while (enumerator.NextItem(out var item, out var slot))
        {
            if (!_immuneQuery.HasComponent(item))
                continue;

            protectedSlots |= slot.SlotFlags & requiredSlots;
        }

        return (protectedSlots & requiredSlots) == requiredSlots;
    }

    private TimeSpan PickDuration(TimeSpan min, TimeSpan max)
    {
        min = ClampDuration(min);
        max = ClampDuration(max);

        if (max < min)
            max = min;

        if (min == max)
            return min;

        return TimeSpan.FromSeconds(_random.NextFloat((float) min.TotalSeconds, (float) max.TotalSeconds));
    }

    private static TimeSpan ClampDamageInterval(LavalandPlanetPrototype planet)
    {
        var interval = ClampDuration(planet.AshStormDamageInterval);
        return interval == TimeSpan.Zero ? TimeSpan.FromSeconds(1) : interval;
    }

    private static TimeSpan ClampDuration(TimeSpan duration)
    {
        return duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
    }
}
