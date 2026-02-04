using Content.Server.Atmos.Components;
using Content.Server.Chat.Managers;
using Content.Server.Shuttles.Components;
using Content.Shared._Frostheim.Map;
using Content.Shared._Frostheim.Roles;
using Content.Shared.Atmos;
using Content.Shared.Chat;
using Content.Shared.Weather;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Frostheim.Map;

public sealed class FrostMapSystem : EntitySystem
{
    [Dependency] private readonly SharedWeatherSystem _weatherSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    private readonly HashSet<EntityUid> _pendingLoot = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostMapComponent, ComponentAdd>(OnFrostMapInit);
        SubscribeLocalEvent<FrostMapComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, FrostMapComponent comp, MapInitEvent args)
    {
        _pendingLoot.Add(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pendingLoot.Count > 0)
        {
            foreach (var uid in _pendingLoot)
            {
                if (TryComp<FrostMapComponent>(uid, out var comp) && !comp.LootSpawned)
                    SpawnLoot(uid, comp);
            }

            _pendingLoot.Clear();
        }

        var query = EntityQueryEnumerator<FrostMapComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextWeatherChange)
                continue;

            var newWeather = comp.CurrentWeather.GetNextWeather(_random.NextFloat());
            ChangeWeather((uid, comp), newWeather);

            comp.NextWeatherChange = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(comp.MinWeatherInterval, comp.MaxWeatherInterval));
        }
    }

    private void SpawnLoot(EntityUid mapUid, FrostMapComponent comp)
    {
        comp.LootSpawned = true;

        var mapId = Transform(mapUid).MapID;

        var thrusterMarkers = new List<EntityUid>();
        var gyroscopeMarkers = new List<EntityUid>();
        var anyMarkers = new List<EntityUid>();

        var markerQuery = EntityQueryEnumerator<FrostheimLootSpawnComponent, TransformComponent>();
        while (markerQuery.MoveNext(out var mUid, out var loot, out var xform))
        {
            if (xform.MapID != mapId)
                continue;

            switch (loot.LootType)
            {
                case FrostheimLootType.Thruster:
                    thrusterMarkers.Add(mUid);
                    break;
                case FrostheimLootType.Gyroscope:
                    gyroscopeMarkers.Add(mUid);
                    break;
                case FrostheimLootType.Any:
                    anyMarkers.Add(mUid);
                    break;
            }
        }

        _random.Shuffle(thrusterMarkers);
        _random.Shuffle(gyroscopeMarkers);
        _random.Shuffle(anyMarkers);

        var thrustersToSpawn = comp.SpawnThrusters;
        var gyroscopesToSpawn = comp.SpawnGyroscopes;

        thrustersToSpawn -= SpawnOnMarkers(thrusterMarkers, comp.BrokenThrusterProto, thrustersToSpawn);
        gyroscopesToSpawn -= SpawnOnMarkers(gyroscopeMarkers, comp.BrokenGyroscopeProto, gyroscopesToSpawn);

        thrustersToSpawn -= SpawnOnMarkers(anyMarkers, comp.BrokenThrusterProto, thrustersToSpawn);
        gyroscopesToSpawn -= SpawnOnMarkers(anyMarkers, comp.BrokenGyroscopeProto, gyroscopesToSpawn);

        var allMarkers = new List<EntityUid>();
        allMarkers.AddRange(thrusterMarkers);
        allMarkers.AddRange(gyroscopeMarkers);
        allMarkers.AddRange(anyMarkers);

        foreach (var marker in allMarkers)
        {
            if (!Deleted(marker))
                QueueDel(marker);
        }
    }

    private int SpawnOnMarkers(List<EntityUid> markers, string prototype, int count)
    {
        var spawned = 0;

        for (var i = markers.Count - 1; i >= 0 && spawned < count; i--)
        {
            var marker = markers[i];
            if (Deleted(marker))
                continue;

            Spawn(prototype, Transform(marker).Coordinates);
            QueueDel(marker);
            markers.RemoveAt(i);
            spawned++;
        }

        return spawned;
    }

    private void OnFrostMapInit(Entity<FrostMapComponent> ent, ref ComponentAdd args)
    {
        ent.Comp.NextWeatherChange = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(ent.Comp.MinWeatherInterval, ent.Comp.MaxWeatherInterval));

        ApplyOutdoorTemperature(ent);
        ApplyIndoorTemperature(ent);
    }

    private void ApplyOutdoorTemperature(Entity<FrostMapComponent> entity)
    {
        if (!TryComp<MapAtmosphereComponent>(entity, out var mapAtmosphere))
            return;

        var newMixture = new GasMixture(mapAtmosphere.Mixture);
        newMixture.Temperature = entity.Comp.CurrentWeather.GetOutdoorTemperature();
        mapAtmosphere.Mixture = newMixture;

        Dirty(entity, mapAtmosphere);
    }

    private void ApplyIndoorTemperature(Entity<FrostMapComponent> entity)
    {
        var mapId = Transform(entity).MapID;
        var query = EntityQueryEnumerator<ShuttleComponent, GridAtmosphereComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var gridAtmos, out var xform))
        {
            if (xform.MapID != mapId)
                continue;

            foreach (var tile in gridAtmos.Tiles.Values)
            {
                if (tile.MapAtmosphere)
                    continue;

                if (tile.Air == null || tile.Air.Immutable)
                    continue;

                tile.Air.Temperature = entity.Comp.CurrentWeather.GetIndoorTemperature();
            }
        }
    }

    private void ChangeWeather(Entity<FrostMapComponent> entity, FrostheimWeather weather)
    {
        if (entity.Comp.CurrentWeather == weather)
            return;

        var oldWeather = entity.Comp.CurrentWeather;
        entity.Comp.CurrentWeather = weather;

        var ev = new FrostheimWeatherChangedEvent(entity, oldWeather, weather);
        RaiseLocalEvent(entity, ref ev);
        Dirty(entity, entity.Comp);

        var mapId = Transform(entity).MapID;
        WeatherPrototype? prototype = null;

        if (weather != FrostheimWeather.None && !_prototypeManager.TryIndex(weather.ToString(), out prototype))
            return;

        _weatherSystem.SetWeather(mapId, prototype, null);

        ApplyOutdoorTemperature(entity);
        ApplyIndoorTemperature(entity);

        NotifyCrew(weather);
    }

    private void NotifyCrew(FrostheimWeather weather)
    {
        var message = Loc.GetString("frostheim-weather-" + weather.ToString().ToLowerInvariant());

        var (color, fontSize) = weather switch
        {
            FrostheimWeather.None => ("#52eb56", 12),
            FrostheimWeather.SnowfallLight => ("#e8d44d", 12),
            FrostheimWeather.SnowfallMedium => ("#eb8c34", 14),
            FrostheimWeather.SnowfallHeavy => ("#eb3434", 16),
            _ => ("#ffffff", 12)
        };

        var wrapped = $"[font size={fontSize}][color={color}][bold]{message}[/bold][/color][/font]";

        var query = EntityQueryEnumerator<FrostheimCrewComponent, ActorComponent>();
        while (query.MoveNext(out _, out _, out var actor))
        {
            _chat.ChatMessageToOne(
                ChatChannel.Server,
                message,
                wrapped,
                default,
                false,
                actor.PlayerSession.Channel);
        }
    }
}
