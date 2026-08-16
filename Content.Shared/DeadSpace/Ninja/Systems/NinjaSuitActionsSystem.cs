using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Actions;

namespace Content.Shared.DeadSpace.Ninja.Systems;

public sealed class NinjaSuitActionsSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NinjaSuitActionsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<NinjaSuitActionsComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<NinjaSuitActionsComponent, GetItemActionsEvent>(OnItemGet);
    }

    private void OnItemGet(Entity<NinjaSuitActionsComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;

        foreach (var action in ent.Comp.ActionEntities)
        {
            args.AddAction(action);
        }
    }

    private void OnMapInit(Entity<NinjaSuitActionsComponent> ent, ref MapInitEvent args)
    {
        foreach (var action in ent.Comp.Actions)
        {
            EntityUid? actionEnt = null;
            _actionContainer.EnsureAction(ent.Owner, ref actionEnt, action);

            if (actionEnt != null)
                ent.Comp.ActionEntities.Add(actionEnt.Value);
        }
    }

    private void OnShutdown(Entity<NinjaSuitActionsComponent> ent, ref ComponentShutdown args)
    {
        foreach (var actionEnt in ent.Comp.ActionEntities)
        {
            _actions.RemoveAction(ent.Owner, actionEnt);
        }
    }
}