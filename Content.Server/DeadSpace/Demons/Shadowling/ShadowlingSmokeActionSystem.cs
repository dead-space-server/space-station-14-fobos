// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingSmokeActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;

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
            args.Handled = true;
        }
    }
}