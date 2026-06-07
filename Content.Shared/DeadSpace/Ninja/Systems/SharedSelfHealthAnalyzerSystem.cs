using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Actions;

namespace Content.Shared.DeadSpace.Ninja.Systems;

public abstract class SharedSelfHealthAnalyzerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SelfHealthAnalyzerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SelfHealthAnalyzerComponent, GetItemActionsEvent>(OnGetActions);
    }

    private void OnMapInit(Entity<SelfHealthAnalyzerComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;
        _actions.AddAction(uid, ref comp.ActionEntity, comp.Action);
        Dirty(uid, comp);
    }

    private void OnGetActions(Entity<SelfHealthAnalyzerComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;
        args.AddAction(ent.Comp.ActionEntity);
    }
}