using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Fluids.Components;
using Content.Shared.Chemistry.Components;
using Robust.Shared.Map;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingSmokeActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

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
