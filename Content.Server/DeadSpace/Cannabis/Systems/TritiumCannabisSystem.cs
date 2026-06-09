using Content.Server.Atmos.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Nutrition.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.DeadSpace.Cannabis;
using Content.Shared.Smoking;

namespace Content.Server.DeadSpace.Cannabis.Systems;

public sealed class TritiumCannabisSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<JointTritiumComponent, SmokableSolutionEmptyEvent>(OnJointEmpty);
        SubscribeLocalEvent<TritiumLeafComponent, MapInitEvent>(OnLeafSpawned);
    }

    private void OnJointEmpty(Entity<JointTritiumComponent> entity, ref SmokableSolutionEmptyEvent args)
    {
        var coords = Transform(entity).MapPosition;
        _explosion.QueueExplosion(coords, "Default", 4f, 3f, 5f, entity);
    }

    private void OnLeafSpawned(Entity<TritiumLeafComponent> entity, ref MapInitEvent args)
    {
        var moles = entity.Comp.Moles;
        if (moles <= 0)
            return;

        var environment = _atmos.GetContainingMixture(entity.Owner, false, true);
        if (environment == null)
            return;

        var merger = new GasMixture(1) { Temperature = Atmospherics.T20C };
        merger.SetMoles(Gas.Tritium, moles);
        _atmos.Merge(environment, merger);

        RemComp<TritiumLeafComponent>(entity);
    }
}
