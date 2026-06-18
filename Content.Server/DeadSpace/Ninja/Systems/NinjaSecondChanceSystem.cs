using Content.Server.Cloning;
using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Server.DeadSpace.Ninja.Systems;

public sealed class NinjaSecondChanceSystem : EntitySystem
{
    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NinjaSecondChanceComponent, MobStateChangedEvent>(OnDeath, before: new[] { typeof(AutoDustSystem) });
    }

    private void OnDeath(Entity<NinjaSecondChanceComponent> ent, ref MobStateChangedEvent args)
    {
        if (!TryComp<AutoDustMarkerComponent>(ent.Owner, out var marker))
            return;

        if (!TryComp<AutoDustComponent>(marker.AutoDustItem, out var dust))
            return;

        if (!((args.NewMobState == MobState.Dead && dust.AutoDustMode == DustMode.Dead) ||
            (args.NewMobState == MobState.Critical && dust.AutoDustMode == DustMode.Crit)))
            return;

        if (ent.Comp.Used)
            return;

        if (!_mind.TryGetMind(ent.Owner, out var mindId, out var mind))
            return;

        var capsuleQuery = EntityQueryEnumerator<NinjaRespawnCapsuleComponent>();
        if (!capsuleQuery.MoveNext(out var capsuleUid, out _))
            return;

        var coords = _transform.GetMapCoordinates(capsuleUid);

        if (!_cloning.TryCloning(ent.Owner, coords, "BaseClone", out var clone))
            return;

        ent.Comp.Used = true;
        Dirty(ent);

        _mind.TransferTo(mindId, clone, ghostCheckOverride: true, mind: mind);
    }
}