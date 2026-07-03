// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System;
using System.Collections.Generic;
using System.Numerics;
using Content.Client.DeadSpace.Afterimages;
using Content.Shared.DeadSpace.Sandevistan;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Client.DeadSpace.Sandevistan;

public sealed class SandevistanAfterimageSystem : EntitySystem
{
    private const int MaxAfterimagesPerFrame = 6;
    private const float MinStepDistance = 0.01f;

    [Dependency] private readonly DeadSpaceAfterimageSystem _afterimages = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private readonly Dictionary<EntityUid, TrailState> _trailStates = new();
    private readonly HashSet<EntityUid> _activeThisFrame = new();
    private readonly List<EntityUid> _staleTrailStates = new();

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        _activeThisFrame.Clear();

        var query = EntityQueryEnumerator<ActiveSandevistanComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var active, out var xform))
        {
            if (Deleted(uid))
                continue;

            _activeThisFrame.Add(uid);
            UpdateTrail(uid, active, xform);
        }

        RemoveStaleTrailStates();
    }

    private void UpdateTrail(EntityUid uid, ActiveSandevistanComponent active, TransformComponent xform)
    {
        var current = _transform.ToMapCoordinates(xform.Coordinates);
        var currentRotation = _transform.GetWorldRotation(xform);

        if (!_trailStates.TryGetValue(uid, out var state) ||
            state.MapId != current.MapId)
        {
            _trailStates[uid] = new TrailState(current.MapId, current.Position, currentRotation);
            SpawnAfterimage(uid, active, xform.Coordinates, currentRotation);
            return;
        }

        var delta = current.Position - state.Position;
        var distance = delta.Length();
        var stepDistance = MathF.Max(active.AfterimageMinDistance, MinStepDistance);

        if (distance < stepDistance)
        {
            _trailStates[uid] = state with
            {
                Rotation = currentRotation,
            };
            return;
        }

        var samples = Math.Clamp((int) MathF.Floor(distance / stepDistance), 1, MaxAfterimagesPerFrame);
        for (var i = 1; i <= samples; i++)
        {
            var fraction = i / (float) samples;
            var position = Vector2.Lerp(state.Position, current.Position, fraction);
            var rotation = Angle.Lerp(state.Rotation, currentRotation, fraction);
            var coordinates = _transform.ToCoordinates(new MapCoordinates(position, current.MapId));

            SpawnAfterimage(uid, active, coordinates, rotation);
        }

        _trailStates[uid] = new TrailState(current.MapId, current.Position, currentRotation);
    }

    private void SpawnAfterimage(
        EntityUid uid,
        ActiveSandevistanComponent active,
        EntityCoordinates coordinates,
        Angle rotation)
    {
        _afterimages.TrySpawnAfterimage(
            uid,
            coordinates,
            rotation,
            active.AfterimageColor,
            active.AfterimageLifetime,
            active.AfterimageFallbackEffect);
    }

    private void RemoveStaleTrailStates()
    {
        _staleTrailStates.Clear();

        foreach (var uid in _trailStates.Keys)
        {
            if (!_activeThisFrame.Contains(uid) || Deleted(uid))
                _staleTrailStates.Add(uid);
        }

        foreach (var uid in _staleTrailStates)
        {
            _trailStates.Remove(uid);
        }
    }

    private readonly record struct TrailState(MapId MapId, Vector2 Position, Angle Rotation);
}
