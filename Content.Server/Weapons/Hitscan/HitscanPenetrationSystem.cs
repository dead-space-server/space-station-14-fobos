using Content.Server.Destructible;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Server.Weapons.Hitscan;

public sealed class HitscanPenetrationSystem : EntitySystem
{
    [Dependency] private readonly DestructibleSystem _destructible = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!; // DS14

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitscanPenetrationComponent, HitscanDamageDealtEvent>(OnHitscanDamageDealt);
    }

    private void OnHitscanDamageDealt(Entity<HitscanPenetrationComponent> ent, ref HitscanDamageDealtEvent args)
    {
        if (args.Data.HitPosition == null || args.Data.ShotDirection.LengthSquared() == 0f)
            return;

        var damageRequired = _destructible.DestroyedAt(args.Target);
        if (TryComp<DamageableComponent>(args.Target, out var damageable))
        {
            var previousDamage = FixedPoint2.Max(damageable.TotalDamage - args.DamageDealt.GetTotal(), FixedPoint2.Zero);
            damageRequired = FixedPoint2.Max(damageRequired - previousDamage, FixedPoint2.Zero);
        }

        if (!TryPenetrate(ent, args, damageRequired))
            return;

        var direction = args.Data.ShotDirection.Normalized();
        var ignored = args.Data.IgnoredEntities == null
            ? new List<EntityUid>()
            : new List<EntityUid>(args.Data.IgnoredEntities);

        if (!ignored.Contains(args.Target))
            ignored.Add(args.Target);

        DestroyNearbyObstacles(ent, args, ignored); // DS14

        var nextCoords = _transform.ToCoordinates(args.Data.HitPosition.Value.Offset(direction * 0.05f));
        var traceEvent = new HitscanTraceEvent
        {
            FromCoordinates = nextCoords,
            ShotDirection = direction,
            Gun = args.Data.Gun,
            Shooter = args.Data.Shooter,
            Target = args.Data.Target,
            OutputTrace = args.Data.OutputTrace,
            IgnoredEntities = ignored,
        };

        RaiseLocalEvent(ent.Owner, ref traceEvent);
    }

    private bool TryPenetrate(Entity<HitscanPenetrationComponent> ent, HitscanDamageDealtEvent args, FixedPoint2 damageRequired)
    {
        if (ent.Comp.PenetrationThreshold == 0 || ent.Comp.PenetratedTargets >= ent.Comp.MaxPenetratedTargets) // DS14
            return false;

        if (ent.Comp.PenetrationDamageTypeRequirement != null)
        {
            foreach (var requiredDamageType in ent.Comp.PenetrationDamageTypeRequirement)
            {
                if (args.DamageDealt.DamageDict.Keys.Contains(requiredDamageType))
                    continue;

                return false;
            }
        }

        if (args.DamageDealt.GetTotal() < damageRequired)
            return false;

        if (ent.Comp.PenetrationAmount + damageRequired > ent.Comp.PenetrationThreshold) // DS14
            return false;

        ent.Comp.PenetrationAmount += damageRequired;
        ent.Comp.PenetratedTargets++; // DS14
        return true; // DS14
    }

    // DS14-start: let a penetrating hit clear a bounded number of adjacent destructible obstructions.
    private void DestroyNearbyObstacles(
        Entity<HitscanPenetrationComponent> ent,
        HitscanDamageDealtEvent args,
        List<EntityUid> ignored)
    {
        if (args.Data.HitPosition == null ||
            ent.Comp.MaxDestroyedObstacles <= 0 ||
            ent.Comp.ObstacleSearchRadius <= 0f)
        {
            return;
        }

        var destroyed = 0;
        foreach (var obstacle in _lookup.GetEntitiesInRange(args.Data.HitPosition.Value, ent.Comp.ObstacleSearchRadius))
        {
            if (obstacle == args.Target ||
                obstacle == ent.Owner ||
                obstacle == args.Data.Shooter ||
                obstacle == args.Data.Gun ||
                ignored.Contains(obstacle) ||
                !TryComp<PhysicsComponent>(obstacle, out var physics) ||
                physics.BodyType != BodyType.Static ||
                !TryComp<DestructibleComponent>(obstacle, out var destructible))
            {
                continue;
            }

            var durability = _destructible.DestroyedAt(obstacle, destructible);
            if (durability <= 0 || ent.Comp.PenetrationAmount + durability > ent.Comp.PenetrationThreshold)
                continue;

            if (!_destructible.DestroyEntity(obstacle))
                continue;

            ignored.Add(obstacle);
            ent.Comp.PenetrationAmount += durability;
            destroyed++;
            if (destroyed >= ent.Comp.MaxDestroyedObstacles)
                break;
        }
    }
    // DS14-end
}
