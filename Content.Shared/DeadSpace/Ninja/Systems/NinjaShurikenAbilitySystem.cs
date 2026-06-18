using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Actions;
using Content.Shared.Cuffs;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Content.Shared.Interaction.Events;

namespace Content.Shared.DeadSpace.Ninja.Systems;

public sealed class NinjaShurikenAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NinjaShurikenAbilityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<NinjaShurikenAbilityComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<NinjaShurikenAbilityComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<NinjaShurikenAbilityComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<NinjaShurikenAbilityComponent, ShurikenHandRetractEvent>(OnRetractableItemAction);

        SubscribeLocalEvent<NinjaShurikenAbilityItemComponent, ComponentShutdown>(OnSummonedShutdown);
        Subs.SubscribeWithRelay<NinjaShurikenAbilityItemComponent, HeldRelayedEvent<TargetHandcuffedEvent>>(OnItemHandcuffed, inventory: false);

        //SubscribeLocalEvent<NinjaShurikenAbilityItemComponent, DroppedEvent>(OnDropAttempt);
    }

    private void OnMapInit(Entity<NinjaShurikenAbilityComponent> ent, ref MapInitEvent args)
    {
        _containers.EnsureContainer<Container>(ent, NinjaShurikenAbilityComponent.ContainerId);
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.Action);

        PopulateActionItem(ent.Owner);
    }

    private void OnGetActions(Entity<NinjaShurikenAbilityComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;

        args.AddAction(ent.Comp.ActionEntity);
    }

    private void OnEquipped(Entity<NinjaShurikenAbilityComponent> ent, ref GotEquippedEvent args)
    {

        if (ent.Comp.ActionEntity == null)
            _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    private void OnUnequipped(Entity<NinjaShurikenAbilityComponent> ent, ref GotUnequippedEvent args)
    {
        if (ent.Comp.ActionItemUid is { } itemUid && _hands.IsHolding(args.Equipee, itemUid))
        {
            RetractRetractableItem(args.Equipee, itemUid, ent.Owner);
        }
    }

    private void OnRetractableItemAction(Entity<NinjaShurikenAbilityComponent> ent, ref ShurikenHandRetractEvent args)
    {
        if (_hands.GetActiveHand(args.Performer) is not { } activeHand)
            return;

        if (_actions.GetAction(ent.Comp.ActionEntity) is not { } action)
            return;

        if (action.Comp.AttachedEntity == null)
            return;

        if (ent.Comp.ActionItemUid == null)
            return;

        var activeItem = _hands.GetActiveItem(args.Performer);

        if (activeItem != null
            && !_hands.IsHolding(args.Performer, ent.Comp.ActionItemUid)
            && !_hands.CanDropHeld(args.Performer, activeHand, false))
        {
            _popups.PopupClient(Loc.GetString("retractable-item-hand-cannot-drop"), args.Performer, args.Performer);
            return;
        }

        if (_hands.IsHolding(args.Performer, ent.Comp.ActionItemUid))
        {
            RetractRetractableItem(args.Performer, ent.Comp.ActionItemUid.Value, ent.Owner);
        }
        else
        {
            SummonRetractableItem(args.Performer, ent.Comp.ActionItemUid.Value, activeHand, ent.Owner);
        }

        args.Handled = true;
    }

    private void OnSummonedShutdown(Entity<NinjaShurikenAbilityItemComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.SummoningAction is not { } summoningAction)
            return;

        if (!TryComp<NinjaShurikenAbilityComponent>(ent.Comp.SummoningAction, out var retract))
            return;

        if (retract.ActionItemUid != ent.Owner)
            return;

        // If the item is somehow destroyed, re-add it to the action.
        PopulateActionItem((summoningAction, retract));
    }

    private void OnItemHandcuffed(Entity<NinjaShurikenAbilityItemComponent> ent, ref HeldRelayedEvent<TargetHandcuffedEvent> args)
    {
        if (!TryComp<NinjaShurikenAbilityComponent>(ent.Comp.SummoningAction, out var retract))
            return;

        if (_actions.GetAction(retract.ActionEntity) is not { } action)
            return;

        if (action.Comp.AttachedEntity == null)
            return;

        if (_hands.GetActiveHand(action.Comp.AttachedEntity.Value) is not { })
            return;

        RetractRetractableItem(action.Comp.AttachedEntity.Value, ent, (ent.Comp.SummoningAction.Value, retract));
    }

    private void PopulateActionItem(Entity<NinjaShurikenAbilityComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false) || TerminatingOrDeleted(ent))
            return;

        if (!PredictedTrySpawnInContainer(ent.Comp.SpawnedPrototype, ent.Owner, NinjaShurikenAbilityComponent.ContainerId, out var summoned))
            return;

        ent.Comp.ActionItemUid = summoned.Value;

        var summonedComp = AddComp<NinjaShurikenAbilityItemComponent>(summoned.Value);
        summonedComp.SummoningAction = ent.Owner;
        Dirty(summoned.Value, summonedComp);

        Dirty(ent);
    }

    private void RetractRetractableItem(EntityUid holder, EntityUid item, Entity<NinjaShurikenAbilityComponent?> action)
    {
        if (!Resolve(action, ref action.Comp, false))
            return;

        RemComp<UnremoveableComponent>(item);
        var container = _containers.GetContainer(action, NinjaShurikenAbilityComponent.ContainerId);
        _containers.Insert(item, container);

    }

    private void SummonRetractableItem(EntityUid holder, EntityUid item, string hand, Entity<NinjaShurikenAbilityComponent?> action)
    {
        if (!Resolve(action, ref action.Comp, false))
            return;

        var container = _containers.GetContainer(action, NinjaShurikenAbilityComponent.ContainerId);
        if (container.Contains(item))
            RemComp<UnremoveableComponent>(item);

        if (!_hands.TryForcePickup(holder, item, hand, checkActionBlocker: false))
            return;

        EnsureComp<UnremoveableComponent>(item);
    }

    //private void OnDropAttempt(Entity<NinjaShurikenAbilityItemComponent> ent, ref DroppedEvent args)
    //{
    //    PredictedQueueDel(ent.Owner);
    //}
}