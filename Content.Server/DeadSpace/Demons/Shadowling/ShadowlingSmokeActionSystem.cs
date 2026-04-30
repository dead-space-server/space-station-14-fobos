// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingSmokeActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingSmokeActionComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingSmokeActionComponent, ShadowlingSmokeActionEvent>(OnSmokeAction);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingSmokeActionComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionSmokeEntity, component.ActionSmoke);
    }

    private void OnSmokeAction(EntityUid uid, ShadowlingSmokeActionComponent component, ShadowlingSmokeActionEvent args)
    {
        if (args.Handled) return;

        var xform = Transform(uid);

        if (xform.GridUid == null)
            return;

        var smoke = Spawn("Smoke", xform.Coordinates);

        if (TryComp<SmokeComponent>(smoke, out var smokeComp))
        {
            _smoke.StartSmoke(smoke, new Solution(), component.SmokeDuration, component.SmokeSpread, smokeComp);

            StartSmokeDamage(smoke, uid, component, smokeComp);
        }

        args.Handled = true;
    }

    private void StartSmokeDamage(EntityUid smoke, EntityUid user, ShadowlingSmokeActionComponent component, SmokeComponent smokeComp)
    {
        var elapsed = 0f;

        while (elapsed < component.SmokeDuration)
        {
            Timer.Spawn(TimeSpan.FromSeconds(elapsed), () =>
            {
                if (!Exists(smoke) || Deleted(smoke))
                    return;

                var smokeMapPos = Transform(smoke).MapPosition;
                var entitiesInSmoke = _lookup.GetEntitiesInRange<MobStateComponent>(smokeMapPos, component.SmokeSpread);

                foreach (var (ent, _) in entitiesInSmoke)
                {
                    if (!Exists(ent) || Deleted(ent))
                        continue;

                    var entMapPos = Transform(ent).MapPosition;

                    if (smokeMapPos.MapId != entMapPos.MapId)
                        continue;

                    var distance = (smokeMapPos.Position - entMapPos.Position).Length();

                    if (distance > component.SmokeSpread)
                        continue;

                    if (HasComp<ShadowlingComponent>(ent) ||
                        HasComp<ShadowlingSlaveComponent>(ent) ||
                        HasComp<ShadowlingRevealComponent>(ent) ||
                        HasComp<ShadowlingRecruitComponent>(ent))
                        continue;

                    var damage = new DamageSpecifier();
                    damage.DamageDict.Add(component.DamageType, component.DamagePerTick);
                    _damageable.TryChangeDamage(ent, damage, true);
                }
            });

            elapsed += component.DamageTickInterval;
        }
    }
}