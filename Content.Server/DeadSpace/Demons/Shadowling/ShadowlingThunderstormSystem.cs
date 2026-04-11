using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using Content.Server.Beam;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;

namespace Content.Server.DeadSpace.Demons.Shadowling
{
    public sealed class ShadowlingThunderstormSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly BeamSystem _beam = default!;
        [Dependency] private readonly SharedStunSystem _stun = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!; // Добавили систему урона

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ShadowlingThunderstormComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<ShadowlingThunderstormComponent, ShadowlingThunderstormEvent>(OnThunderstormAction);
        }

        private void OnComponentInit(EntityUid uid, ShadowlingThunderstormComponent component, ComponentInit args)
        {
            _actions.AddAction(uid, ref component.ActionThunderstormEntity, component.ActionThunderstorm);
        }

        private void OnThunderstormAction(EntityUid uid, ShadowlingThunderstormComponent component, ShadowlingThunderstormEvent args)
        {
            if (args.Handled)
                return;

            var target = args.Target;

            if (!HasComp<MobStateComponent>(target) || _mobState.IsDead(target) || HasComp<ShadowlingComponent>(target) || HasComp<ShadowlingSlaveComponent>(target))
                return;

            args.Handled = true;

            var struck = new List<EntityUid> { uid };
            var source = uid;
            var current = target;

            for (var i = 0; i != component.MaxTargets; i++)
            {
                _beam.TryCreateBeam(source, current, component.LightningPrototype);

                _stun.TryUpdateParalyzeDuration(current, TimeSpan.FromSeconds(component.StunDuration));

                var damage = new DamageSpecifier();
                damage.DamageDict.Add("Shock", 25);
                _damageable.TryChangeDamage(current, damage, true);

                struck.Add(current);

                var xform = Transform(current);
                var mapPos = _transform.GetMapCoordinates(current, xform);

                var nearby = _lookup.GetEntitiesInRange<MobStateComponent>(mapPos, component.Range);

                EntityUid? next = null;
                var dist = float.MaxValue;

                foreach (var (ent, _) in nearby)
                {
                    if (struck.Contains(ent) || _mobState.IsDead(ent) || HasComp<ShadowlingComponent>(ent) || HasComp<ShadowlingSlaveComponent>(ent))
                        continue;

                    var curDist = (_transform.GetWorldPosition(ent) - _transform.GetWorldPosition(current)).LengthSquared();
                    if (dist > curDist)
                    {
                        dist = curDist;
                        next = ent;
                    }
                }

                if (next == null)
                    break;

                source = current;
                current = next.Value;
            }
        }
    }
}
