using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Server.Temperature.Components;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Content.Shared.Damage;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingFreezingVeinsSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingFreezingVeinsComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingFreezingVeinsComponent, ShadowlingFreezingVeinsEvent>(OnFreezingVeinsAction);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingFreezingVeinsComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionFreezingVeinsEntity, component.ActionFreezingVeins);
    }

    private void OnFreezingVeinsAction(EntityUid uid, ShadowlingFreezingVeinsComponent component, ShadowlingFreezingVeinsEvent args)
    {
        if (args.Handled) return;

        var target = args.Target;

        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        if (HasComp<ShadowlingComponent>(target) ||
            HasComp<ShadowlingRevealComponent>(target) ||
            HasComp<ShadowlingSlaveComponent>(target))
            return;

        if (TryComp<TemperatureComponent>(target, out var temp))
        {
            temp.CurrentTemperature = component.TemperatureSet;
        }

        DamageSpecifier damage = new();
        damage.DamageDict.Add("Cold", component.DamageCold);
        _damageable.TryChangeDamage(target, damage, true);

        _popup.PopupEntity("Кровь в ваших венах начинает замерзать!", target, target, PopupType.LargeCaution);

        args.Handled = true;
    }
}
