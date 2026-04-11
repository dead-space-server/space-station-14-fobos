using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Mobs;
using Content.Shared.FixedPoint;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingBlackMedSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingBlackMedComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingBlackMedEvent>(OnBlackMedAction);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingBlackMedComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionBlackMedEntity, component.ActionBlackMed);
    }

    private void OnBlackMedAction(ShadowlingBlackMedEvent args)
    {
        if (args.Handled) return;

        var target = args.Target;

        if (!HasComp<ShadowlingSlaveComponent>(target))
        {
            return;
        }

        if (TryComp<DamageableComponent>(target, out var damageable))
        {
            _damageable.SetAllDamage(target, damageable, FixedPoint2.Zero);
        }

        if (_mobState.IsDead(target) || _mobState.IsCritical(target))
        {
            _mobState.ChangeMobState(target, MobState.Alive);
        }

        _popup.PopupEntity("Тёмная энергия восстанавливает ваше тело!", target, target, PopupType.Medium);

        args.Handled = true;
    }
}
