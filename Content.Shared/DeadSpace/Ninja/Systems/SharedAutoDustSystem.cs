using Content.Shared.Actions;
using Content.Shared.DeadSpace.Ninja.Components;

namespace Content.Shared.DeadSpace.Ninja.Systems;

public abstract class SharedAutoDustSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoDustComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AutoDustComponent, GetItemActionsEvent>(OnGetActions);
    }

    private void OnMapInit(Entity<AutoDustComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;
        _actions.AddAction(uid, ref comp.ActionEntity, comp.Action);
        Dirty(uid, comp);
    }

    private void OnGetActions(Entity<AutoDustComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;
        args.AddAction(ent.Comp.ActionEntity);
    }
}