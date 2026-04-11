using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Robust.Server.GameObjects;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingPhaseSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingPhaseComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShadowlingPhaseComponent, ShadowlingPhaseActionEvent>(OnShadowlingPhase);
    }

    private void OnMapInit(EntityUid uid, ShadowlingPhaseComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, "ActionShadowlingPhase");
    }

    private void OnShadowlingPhase(EntityUid uid, ShadowlingPhaseComponent component, ShadowlingPhaseActionEvent args)
    {
        if (args.Handled) return;

        var result = _polymorph.PolymorphEntity(uid, "ShadowlingPhasePolymorph");
        if (result != null)
        {
            _eye.SetDrawFov(result.Value, false);
            _eye.SetDrawLight(result.Value, false);
        }
        args.Handled = true;
    }
}
