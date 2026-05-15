// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.DeadSpace.ThermalVision;

namespace Content.Server.DeadSpace.ThermalVision;

public sealed class ThermalVisionActiveSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ThermalVisionActiveComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ThermalVisionActiveComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<ThermalVisionActiveComponent, ToggleThermalVisionActionEvent>(OnToggle);
    }

    private void OnStartup(EntityUid uid, ThermalVisionActiveComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ActionToggleThermalVisionEntity, component.ActionToggleThermalVision);
    }

    private void OnRemove(EntityUid uid, ThermalVisionActiveComponent component, ComponentRemove args)
    {
        _actions.RemoveAction(uid, component.ActionToggleThermalVisionEntity);
    }

    private void OnToggle(EntityUid uid, ThermalVisionActiveComponent component, ToggleThermalVisionActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        component.IsActive = !component.IsActive;
        Dirty(uid, component);
    }
}