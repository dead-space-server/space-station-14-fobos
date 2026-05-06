// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using System.Numerics;
using Content.Shared.DeadSpace.Abilities;
using Content.Shared.Ghost;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Server.DeadSpace.Abilities.Systems;

public sealed class ThrowingThingsSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ThrowingThingsActionEvent>(OnAction);
    }

    private void OnAction(ThrowingThingsActionEvent args)
    {
        if (args.Handled)
            return;

        var targetMapPos = _transform.ToMapCoordinates(args.Target);
        if (targetMapPos.MapId == MapId.Nullspace)
            return;

        var uid = args.Performer;
        var performerMapPos = _transform.GetMapCoordinates(uid);
        var targetPos = targetMapPos.Position;

        var items = new List<(EntityUid Item, float DistanceSquared)>();

        foreach (var item in _lookup.GetEntitiesInRange(performerMapPos, args.Range))
        {
            if (item == uid)
                continue;
            if (!CanThrow(item))
                continue;
            if (!TryComp<PhysicsComponent>(item, out var physics) || physics.BodyType == BodyType.Static)
                continue;

            if (args.Entities.Count > 0)
            {
                var prototype = MetaData(item).EntityPrototype;
                if (prototype == null)
                    continue;
                if (!args.Entities.Contains(prototype.ID))
                    continue;
            }

            var itemPos = _transform.GetMapCoordinates(item).Position;
            items.Add((item, Vector2.DistanceSquared(performerMapPos.Position, itemPos)));
        }

        items.Sort(static (left, right) => left.DistanceSquared.CompareTo(right.DistanceSquared));

        var count = Math.Min(items.Count, args.HowMuch);

        for (var i = 0; i < count; i++)
        {
            var item = items[i].Item;
            var itemPos = _transform.GetMapCoordinates(item).Position;

            var diff = targetPos - itemPos;
            if (diff.LengthSquared() < 0.0001f)
                diff = Vector2.UnitY;

            if (HasComp<ProjectileComponent>(item))
            {
                LaunchProjectile(item, diff, args.ThrowStrength, uid);
            }
            else
            {
                _throwing.TryThrow(
                    item,
                    diff,
                    args.ThrowStrength,
                    uid,
                    recoil: false,
                    compensateFriction: false);
            }
        }

        args.Handled = true;
    }
    private void LaunchProjectile(EntityUid item, Vector2 direction, float strength, EntityUid shooter)
    {
        if (!TryComp<PhysicsComponent>(item, out var physics))
            return;

        _physics.SetBodyType(item, BodyType.Dynamic, body: physics);

        var velocity = Vector2.Normalize(direction) * strength;
        _physics.SetLinearVelocity(item, velocity, body: physics);

        // Указываем стрелка, чтобы снаряд не ударил самого исполнителя
        if (TryComp<ProjectileComponent>(item, out var projectile))
        {
            projectile.Shooter = shooter;
            projectile.Weapon = shooter;
        }
    }

    private bool CanThrow(EntityUid uid)
    {
        return !(
            HasComp<GhostComponent>(uid) ||
            HasComp<MapGridComponent>(uid) ||
            HasComp<MapComponent>(uid)
        );
    }
}