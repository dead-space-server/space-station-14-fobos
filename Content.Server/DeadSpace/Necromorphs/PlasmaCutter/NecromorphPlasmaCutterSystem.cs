// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DeadSpace.Necromorphs.InfectionDead.Components;
using Content.Shared.DeadSpace.Necromorphs.PlasmaCutter;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Hitscan.Events;
using Robust.Shared.Containers;
using System.Linq;

namespace Content.Server.DeadSpace.Necromorphs.PlasmaCutter;

public sealed class NecromorphPlasmaCutterSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NecromorphPlasmaCutterComponent, HitscanDamageDealtEvent>(OnHit);
        SubscribeLocalEvent<NecromorphPlasmaCutterWoundsComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    private void OnRefreshSpeed(Entity<NecromorphPlasmaCutterWoundsComponent> ent,
        ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.RemovedLegs == 1)
            args.ModifySpeed(0.5f);
        else if (ent.Comp.RemovedLegs >= 2)
            args.ModifySpeed(0.1f);
    }

    private void OnHit(Entity<NecromorphPlasmaCutterComponent> ent, ref HitscanDamageDealtEvent args)
    {
        var target = args.Target;
        if (!HasComp<NecromorfComponent>(target))
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target) || !HasComp<BodyComponent>(target))
        {
            DamageNonHumanoid(target);
            return;
        }

        var wounds = EnsureComp<NecromorphPlasmaCutterWoundsComponent>(target);
        if (wounds.RemovedLegs < 2 && TryDetachFirstPart(target, BodyPartType.Leg))
        {
            wounds.RemovedLegs++;
            Dirty(target, wounds);
            _movement.RefreshMovementSpeedModifiers(target);
            return;
        }

        if (!TryDetachFirstPart(target, BodyPartType.Head))
            return;

        EnsureComp<NecromorphMissingHeadComponent>(target);
        _mobState.ChangeMobState(target, MobState.Dead);
    }

    private bool TryDetachFirstPart(EntityUid target, BodyPartType type)
    {
        var part = _body.GetBodyChildrenOfType(target, type).FirstOrDefault();
        if (part.Id == EntityUid.Invalid ||
            !_containers.TryGetContainingContainer((part.Id, null, null), out var container))
        {
            return false;
        }

        return _containers.Remove(part.Id, container, force: true);
    }

    private void DamageNonHumanoid(EntityUid target)
    {
        var state = EnsureComp<NecromorphPlasmaCutterDamageComponent>(target);
        state.Hits++;

        if (state.Hits >= 3)
        {
            _mobState.ChangeMobState(target, MobState.Dead);
            return;
        }

        if (!TryComp<DamageableComponent>(target, out var damageable) ||
            !TryComp<MobThresholdsComponent>(target, out var thresholds))
        {
            return;
        }

        var deathThreshold = thresholds.Thresholds
            .Where(pair => pair.Value == MobState.Dead)
            .Select(pair => pair.Key.Float())
            .DefaultIfEmpty(damageable.TotalDamage.Float())
            .Min();
        var health = Math.Max(0f, deathThreshold - damageable.TotalDamage.Float());
        var fraction = state.Hits == 1 ? 0.5f : 0.35f;
        var damage = new DamageSpecifier
        {
            DamageDict = { ["Cellular"] = FixedPoint2.New(health * fraction) }
        };
        _damage.TryChangeDamage(target, damage, true, false);
    }
}
