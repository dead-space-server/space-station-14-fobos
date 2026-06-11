using Content.Shared.Actions;
using Content.Shared.DeadSpace.Triggers.Components;
using Content.Shared.Trigger;

namespace Content.Shared.DeadSpace.Triggers.Systems;

public sealed class TriggerOnActionSystem : TriggerOnXSystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnActionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TriggerOnActionComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<TriggerOnActionComponent, TriggerActionEvent>(OnTriggerAction);
        SubscribeLocalEvent<TriggerOnActionComponent, GetItemActionsEvent>(OnGetActions);
    }

    private void OnMapInit(Entity<TriggerOnActionComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;

        _actions.AddAction(uid, ref comp.ActionEntity, comp.Action);
    }

    private void OnGetActions(Entity<TriggerOnActionComponent> ent, ref GetItemActionsEvent args)
    {
        if (!ent.Comp.Parent)
            return;

        args.AddAction(ent.Comp.ActionEntity);
    }

    private void OnTriggerAction(Entity<TriggerOnActionComponent> ent, ref TriggerActionEvent args)
    {
        Trigger.Trigger(ent.Owner, args.Performer, ent.Comp.KeyOut);
        if (ent.Comp.DeleteComponentAfterTrigger)
        {
            RemComp<TriggerOnActionComponent>(ent.Owner);
        }
        args.Handled = true;
    }

    private void OnComponentShutdown(Entity<TriggerOnActionComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEntity);
    }
}
