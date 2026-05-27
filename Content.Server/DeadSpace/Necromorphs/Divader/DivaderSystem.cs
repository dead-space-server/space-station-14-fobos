// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Server.DeadSpace.Divader;

public sealed class DivaderSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DivaderComponent, MobStateChangedEvent>(OnState);
    }

    private void OnState(EntityUid uid, DivaderComponent component, MobStateChangedEvent args)
    {
        if (_mobState.IsDead(uid))
        {
            var xform = Transform(uid);
            var coordinates = _transform.GetMapCoordinates(uid, xform);
            var rotation = _transform.GetWorldRotation(xform);

            Spawn(component.RHMobSpawnId, coordinates, rotation: rotation);
            Spawn(component.HMobSpawnId, coordinates, rotation: rotation);
            Spawn(component.LHMobSpawnId, coordinates, rotation: rotation);
        }
    }
}
