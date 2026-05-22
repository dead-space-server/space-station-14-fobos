using System.Numerics;
using Content.Shared.Maps;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Shared.Trigger.Systems;

public sealed class ScramOnTriggerSystem : XOnTriggerSystem<ScramOnTriggerComponent>
{
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly TurfSystem _turfSystem = default!;

    protected override void OnTrigger(Entity<ScramOnTriggerComponent> ent, EntityUid target, ref TriggerEvent args)
    {
        // We need stop the user from being pulled so they don't just get "attached" with whoever is pulling them.
        // This can for example happen when the user is cuffed and being pulled.
        if (TryComp<PullableComponent>(target, out var pull) && _pulling.IsPulled(target, pull))
            _pulling.TryStopPull(target, pull);

        // Check if the user is pulling anything, and drop it if so.
        if (TryComp<PullerComponent>(target, out var puller) && TryComp<PullableComponent>(puller.Pulling, out var pullable))
            _pulling.TryStopPull(puller.Pulling.Value, pullable);

        _audio.PlayPredicted(ent.Comp.TeleportSound, ent, args.User);

        // Can't predict picking random grids and the target location might be out of PVS range.
        if (_net.IsClient)
            return;

        var targetCoords = SelectRandomTileInFacingArea(target, ent.Comp.TeleportRadius); // DS14

        if (targetCoords != null)
        {
            _transform.SetCoordinates(target, targetCoords.Value);
            args.Handled = true;
        }
    }
    // DS14-start
    /// <summary>
    /// Finds an empty tile inside the area in front of the entity. Will not select off-grid tiles.
    /// </summary>
    /// <remarks> Trends towards the outer distance. Compensates for small grids. </remarks>
    private EntityCoordinates? SelectRandomTileInFacingArea(EntityUid uid, Vector2 radius, int tries = 40, PhysicsComponent? physicsComponent = null)
    {
        var userXform = Transform(uid);
        var userCoords = userXform.Coordinates;
        EntityCoordinates? targetCoords = null;

        if (!Resolve(uid, ref physicsComponent))
            return targetCoords;

        var forward = userXform.LocalRotation.ToWorldVec().Normalized();
        var side = new Vector2(-forward.Y, forward.X);
        var minDistance = MathF.Max(1f, radius.X);
        var maxDistance = MathF.Max(minDistance, radius.Y);

        for (var i = 0; i < tries; i++)
        {
            // distance = r * sq(x) * i
            // r = the radius of the search area.
            // sq(x) = the square root of [0 - 1]. Gives a number trending to the
            // upper range of [0, 1] so that you tend to teleport further.
            // i = A percentage based on the current try count, which results in each
            // subsequent try landing closer and closer towards the entity.
            // Beneficial for smaller maps, especially when the radius is large.
            var distance = (maxDistance - minDistance) * MathF.Sqrt(_random.NextFloat()) * (1 - (float)i / tries) + minDistance;

            // The user is the rear edge of the target area: depth is measured forward from their view direction.
            var lateralOffset = _random.NextFloat(-distance / 2f, distance / 2f);
            var tempTargetCoords = userCoords.Offset(forward * distance + side * lateralOffset);

            if (!_turfSystem.TryGetTileRef(tempTargetCoords, out var tileRef)
                || _turfSystem.IsSpace(tileRef.Value)
                || _turfSystem.IsTileBlocked(tileRef.Value, (CollisionGroup)physicsComponent.CollisionMask))
                continue;

            targetCoords = tempTargetCoords;
            break;
        }

        return targetCoords;
    }
    // DS14-end
}
