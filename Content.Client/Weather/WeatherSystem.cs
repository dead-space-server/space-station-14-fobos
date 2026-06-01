using System.Numerics;
using Content.Client.Gameplay;
using Content.Shared.CCVar;
using Content.Shared.Light.Components;
using Content.Shared.Weather;
using Robust.Shared.Audio;
using Robust.Client.Audio;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using AudioComponent = Robust.Shared.Audio.Components.AudioComponent;

namespace Content.Client.Weather;

public sealed class WeatherSystem : SharedWeatherSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!; // DS14
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IStateManager _state = default!; // DS14

    private float _ambienceVolume; // DS14

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_cfg, CCVars.AmbienceVolume, SetAmbienceVolume, true); // DS14
        SubscribeLocalEvent<WeatherComponent, ComponentHandleState>(OnWeatherHandleState);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnLocalPlayerDetached); // DS14
    }

    protected override void Run(EntityUid uid, WeatherData weather, WeatherPrototype weatherProto, float frameTime)
    {
        base.Run(uid, weather, weatherProto, frameTime);

        if (!TryGetWeatherAudioTarget(uid, out var entXform)) // DS14
        {
            weather.Stream = _audio.Stop(weather.Stream);
            return;
        }

        if (!Timing.IsFirstTimePredicted || weatherProto.Sound == null)
            return;

        weather.Stream ??= PlayWeatherAudio(weatherProto); // DS14

        if (!TryComp(weather.Stream, out AudioComponent? comp))
            return;

        var occlusion = 0f;

        // Work out tiles nearby to determine volume.
        if (TryComp<MapGridComponent>(entXform.GridUid, out var grid))
        {
            TryComp(entXform.GridUid, out RoofComponent? roofComp);
            var gridId = entXform.GridUid.Value;
            // FloodFill to the nearest tile and use that for audio.
            var seed = _mapSystem.GetTileRef(gridId, grid, entXform.Coordinates);
            var frontier = new Queue<TileRef>();
            frontier.Enqueue(seed);
            // If we don't have a nearest node don't play any sound.
            EntityCoordinates? nearestNode = null;
            var visited = new HashSet<Vector2i>();

            while (frontier.TryDequeue(out var node))
            {
                if (!visited.Add(node.GridIndices))
                    continue;

                if (!CanWeatherAffect(entXform.GridUid.Value, grid, node, roofComp))
                {
                    // Add neighbors
                    // TODO: Ideally we pick some deterministically random direction and use that
                    // We can't just do that naively here because it will flicker between nearby tiles.
                    for (var x = -1; x <= 1; x++)
                    {
                        for (var y = -1; y <= 1; y++)
                        {
                            if (Math.Abs(x) == 1 && Math.Abs(y) == 1 ||
                                x == 0 && y == 0 ||
                                (new Vector2(x, y) + node.GridIndices - seed.GridIndices).Length() > 3)
                            {
                                continue;
                            }

                            frontier.Enqueue(_mapSystem.GetTileRef(gridId, grid, new Vector2i(x, y) + node.GridIndices));
                        }
                    }

                    continue;
                }

                nearestNode = new EntityCoordinates(entXform.GridUid.Value,
                    node.GridIndices + grid.TileSizeHalfVector);
                break;
            }

            // Get occlusion to the targeted node if it exists, otherwise set a default occlusion.
            if (nearestNode != null)
            {
                var entPos = _transform.GetMapCoordinates(entXform);
                var nodePosition = _transform.ToMapCoordinates(nearestNode.Value).Position;
                var delta = nodePosition - entPos.Position;
                var distance = delta.Length();
                occlusion = _audio.GetOcclusion(entPos, delta, distance);
            }
            else
            {
                occlusion = 3f;
            }
        }

        var alpha = GetPercent(weather, uid);
        alpha *= SharedAudioSystem.VolumeToGain(weatherProto.Sound.Params.Volume + _ambienceVolume); // DS14
        _audio.SetGain(weather.Stream, alpha, comp);
        comp.Occlusion = occlusion;
    }

    // DS14-start
    private void SetAmbienceVolume(float value)
    {
        _ambienceVolume = SharedAudioSystem.GainToVolume(value);
    }

    private bool TryGetWeatherAudioTarget(EntityUid uid, out TransformComponent entXform)
    {
        entXform = default!;

        if (_state.CurrentState is not GameplayState)
            return false;

        var ent = _playerManager.LocalEntity;
        if (ent == null)
            return false;

        var mapUid = Transform(uid).MapUid;
        if (mapUid == null)
            return false;

        entXform = Transform(ent.Value);
        return entXform.MapUid == mapUid;
    }

    private EntityUid? PlayWeatherAudio(WeatherPrototype weatherProto)
    {
        if (weatherProto.Sound == null)
            return null;

        var audioParams = weatherProto.Sound.Params.WithVolume(weatherProto.Sound.Params.Volume + _ambienceVolume);
        return _audio.PlayGlobal(weatherProto.Sound, Filter.Local(), true, audioParams)?.Entity;
    }

    private void OnLocalPlayerDetached(LocalPlayerDetachedEvent args)
    {
        StopAllWeatherAudio();
    }

    private void StopAllWeatherAudio()
    {
        var query = EntityQueryEnumerator<WeatherComponent>();
        while (query.MoveNext(out _, out var component))
        {
            foreach (var weather in component.Weather.Values)
            {
                weather.Stream = _audio.Stop(weather.Stream);
            }
        }
    }
    // DS14-end

    protected override bool SetState(EntityUid uid, WeatherState state, WeatherComponent comp, WeatherData weather, WeatherPrototype weatherProto)
    {
        if (!base.SetState(uid, state, comp, weather, weatherProto))
            return false;

        if (!Timing.IsFirstTimePredicted)
            return true;

        // TODO: Fades (properly)
        weather.Stream = _audio.Stop(weather.Stream);

        // DS14-start
        if (TryGetWeatherAudioTarget(uid, out _))
            weather.Stream = PlayWeatherAudio(weatherProto);
        // DS14-end

        return true;
    }

    private void OnWeatherHandleState(EntityUid uid, WeatherComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not WeatherComponentState state)
            return;

        foreach (var (proto, weather) in component.Weather)
        {
            // End existing one
            if (!state.Weather.TryGetValue(proto, out var stateData))
            {
                EndWeather(uid, component, proto);
                continue;
            }

            // Data update?
            weather.StartTime = stateData.StartTime;
            weather.EndTime = stateData.EndTime;
            weather.State = stateData.State;
        }

        foreach (var (proto, weather) in state.Weather)
        {
            if (component.Weather.ContainsKey(proto))
                continue;

            // New weather
            StartWeather(uid, component, ProtoMan.Index<WeatherPrototype>(proto), weather.EndTime);
        }
    }
}
