using Content.Server.Atmos.Components;
using Content.Server.Shuttles.Components;
using Content.Shared._Frostheim;
using Content.Shared.Atmos;
using Robust.Shared.Map;

namespace Content.Server._Frostheim;

public sealed class FrostMapSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostMapComponent, ComponentAdd>(OnFrostMapInit);
    }

    private void OnFrostMapInit(Entity<FrostMapComponent> ent, ref ComponentAdd args)
    {
        ApplyOutdoorTemperature(ent);
        ApplyIndoorTemperature(ent);
    }

    public void ApplyOutdoorTemperature(Entity<FrostMapComponent> entity)
    {
        if (!TryComp<MapAtmosphereComponent>(entity, out var mapAtmosphere))
            return;

        var newMixture = new GasMixture(mapAtmosphere.Mixture);
        newMixture.Temperature = entity.Comp.CurrentWeather.GetOutdoorTemperature();
        mapAtmosphere.Mixture = newMixture;

        Dirty(entity, mapAtmosphere);
    }

    public void ApplyIndoorTemperature(Entity<FrostMapComponent> entity)
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
}
