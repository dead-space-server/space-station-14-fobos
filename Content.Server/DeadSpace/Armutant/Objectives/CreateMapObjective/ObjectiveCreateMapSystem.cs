using System.Numerics;
using Content.Server.DeadSpace.Armutant.Base.Components;
using Content.Server.DeadSpace.Armutant.Objectives.CreateMapObjective.Components;
using Content.Shared.Interaction.Events;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Console;
using Robust.Shared.Random;

namespace Content.Server.DeadSpace.Armutant.Objectives.CreateMapObjective;

public sealed class ObjectiveCreateMapSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ObjectiveCreateMapComponent, UseInHandEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, ObjectiveCreateMapComponent component, UseInHandEvent args)
    {
        var user = args.User;

        if (!_entities.HasComponent<ArmutantArmsComponent>(user))
            return;

        int randomMapId;
        MapId mapId;
        do
        {
            randomMapId = _random.Next(10000, 99999);
            mapId = new MapId(randomMapId);
        }
        while (_map.MapExists(mapId));

        component.MapId = randomMapId;

        CreateMap(uid, component);

        var mapUid = _map.GetMapEntityId(mapId);
        if (!_entities.EntityExists(mapUid))
            return;

        if (!_entities.TryGetComponent<TransformComponent>(user, out var userTransform))
            return;

        userTransform.Coordinates = new EntityCoordinates(mapUid, Vector2.Zero);

        SpawnAttachedTo(component.SelfEffect, Transform(uid).Coordinates);
    }

    public void CreateMap(EntityUid uid, ObjectiveCreateMapComponent component)
    {
        var mapId = new MapId(component.MapId);

        if (_map.MapExists(mapId))
            return;

        var mapLoader = _entities.System<MapLoaderSystem>();
        if (string.IsNullOrWhiteSpace(component.MapPath))
            return;

        var loadOptions = new MapLoadOptions { StoreMapUids = true };
        if (!mapLoader.TryLoad(mapId, component.MapPath, out _, loadOptions))
            return;

        _consoleHost.ExecuteCommand($"mapinit {component.MapId}");
    }
}

