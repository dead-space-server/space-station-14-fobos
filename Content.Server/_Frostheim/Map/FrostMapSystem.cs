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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostMapComponent, ComponentAdd>(OnFrostMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

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

        entity.Comp.CurrentWeather = weather;
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
