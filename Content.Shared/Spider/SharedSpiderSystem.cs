using Content.Shared.Actions;

namespace Content.Shared.Spider;

public abstract class SharedSpiderSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiderComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SpiderComponent, ComponentShutdown>(OnShutdown); // DS14
    }

    private void OnInit(EntityUid uid, SpiderComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, ref component.Action, component.WebAction, uid);
    }

    // DS14-start
    private void OnShutdown(EntityUid uid, SpiderComponent component, ComponentShutdown args)
    {
        _action.RemoveAction(uid, component.Action);
    }
    // DS14-end
}
