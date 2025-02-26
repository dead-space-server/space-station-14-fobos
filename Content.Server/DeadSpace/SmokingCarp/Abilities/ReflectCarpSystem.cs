using Content.Server.DeadSpace.SmokingCarp.Abilities.Components;
using Content.Shared.Actions;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.DeadSpace.SmokingCarp;
using Content.Shared.Popups;
using Content.Shared.Weapons.Reflect;

namespace Content.Server.DeadSpace.SmokingCarp.Abilities;

public sealed class ReflectCarpSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReflectCarpComponent, ReflectCarpEvent>(OnToggleReflect);
        SubscribeLocalEvent<ReflectCarpComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ReflectCarpComponent, ComponentShutdown>(OnComponentShutdown);
    }
    private void OnComponentInit(EntityUid uid, ReflectCarpComponent component, ComponentInit args)
    {
        _action.AddAction(uid, ref component.ActionReflectCarpEntity, component.ActionReflectCarp, uid);
    }

    private void OnComponentShutdown(EntityUid uid, ReflectCarpComponent component, ComponentShutdown args)
    {
        _action.RemoveAction(uid, component.ActionReflectCarpEntity);
    }

    private void OnToggleReflect(EntityUid uid, ReflectCarpComponent component, ReflectCarpEvent args)
    {
        if (HasComp<ReflectComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("unreflect-smoking-carp"), uid, uid);

            RemComp<ReflectComponent>(uid);

            if (HasComp<PacifiedComponent>(uid))
                RemComp<PacifiedComponent>(uid);

            return;
        }

        if (args.Handled)
            return;

        args.Handled = true;

        EnsureComp<PacifiedComponent>(uid);
        var reflectComponent = EnsureComp<ReflectComponent>(uid);
        _popup.PopupEntity(Loc.GetString("reflect-smoking-carp"), uid, uid);
        reflectComponent.ReflectProb = 1.0f;
        reflectComponent.Spread = 360f;
    }
}
