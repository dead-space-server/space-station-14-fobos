using System.Linq;
using Content.Server.DeadSpace.SmokingCarp.Abilities.Components;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DeadSpace.SmokingCarp;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Popups;

namespace Content.Server.DeadSpace.SmokingCarp.Abilities;

public sealed class SmokePunchSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmokePunchComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<SmokePunchComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<SmokePunchComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<SmokePunchComponent, SmokePunchCarpEvent>(OnActionActivated);
    }
    private void OnComponentInit(EntityUid uid, SmokePunchComponent component, ComponentInit args)
    {
        _actionSystem.AddAction(uid, ref component.ActionSmokePunchCarpAttackEntity, component.ActionSmokePunchCarpAttack, uid);
    }
    private void OnComponentShutdown(EntityUid uid, SmokePunchComponent component, ComponentShutdown args)
    {
        _actionSystem.RemoveAction(uid, component.ActionSmokePunchCarpAttackEntity);
    }
    private void OnActionActivated(EntityUid uid, SmokePunchComponent component, SmokePunchCarpEvent args)
    {
        if (args.Handled)
            return;

        if (component.IsDamageAttack)
            _popup.PopupEntity(Loc.GetString("non-active-smoking-carp"), uid, uid);

        if (!component.IsDamageAttack)
            _popup.PopupEntity(Loc.GetString("active-smoking-carp"), uid, uid);

        component.IsDamageAttack = !component.IsDamageAttack;

        args.Handled = true;
    }
    private void OnMeleeHit(EntityUid uid, SmokePunchComponent component, MeleeHitEvent args)
    {
        if (component.IsDamageAttack && args.HitEntities.Any())
        {
            foreach (var target in args.HitEntities)
            {
                if (args.User == target)
                    continue;

                if (!TryComp<MobStateComponent>(target, out var mobState))
                    continue;

                if (mobState.CurrentState != MobState.Dead)
                {
                    _damageable.TryChangeDamage(target, component.Damage, true, false);

                    if (TryComp<StaminaComponent>(target, out var stamina))
                        _staminaSystem.TakeStaminaDamage(target, 30);

                    SpawnAttachedTo(component.EffectPunch, Transform(target).Coordinates);

                    component.IsDamageAttack = false;
                }
            }
        }
    }
}
