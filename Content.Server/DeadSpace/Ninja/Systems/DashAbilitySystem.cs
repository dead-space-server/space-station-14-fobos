using Content.Shared.DeadSpace.Ninja.Systems;
using Robust.Shared.Map;
using Content.Server.Beam;

namespace Content.Server.DeadSpace.Ninja.Systems;

public sealed class DashAbilitySystem : SharedDashAbilitySystem
{
    [Dependency] private readonly BeamSystem _beam = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    protected override void DoTeleport(EntityUid user, EntityCoordinates target, string? beam = null)
    {
        var xform = Transform(user);
        var point = Spawn("BeamStartPoint", xform.Coordinates);
        _transform.SetCoordinates(user, xform, target);
        _transform.AttachToGridOrMap(user, xform);

        if (beam != null)
            _beam.TryCreateBeam(point, user, beam);
    }
}