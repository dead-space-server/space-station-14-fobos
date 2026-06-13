using Content.Shared.Actions;
using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Inventory.Events;

namespace Content.Shared.DeadSpace.Ninja.Systems;

public abstract class SharedNinjaCloakSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NinjaCloakComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<SpaceNinjaComponent, ToggleCloakNinjaEvent>(OnNinjaToggleCloak);
        SubscribeLocalEvent<NinjaCloakComponent, GotUnequippedEvent>(OnUnequipped);
    }


    private void OnNinjaToggleCloak(Entity<SpaceNinjaComponent> ent, ref ToggleCloakNinjaEvent args)
    {
        args.Handled = true;
        if (ent.Comp.Suit is not { } suitUid)
            return;

        if (!TryComp<NinjaCloakComponent>(suitUid, out var cloak))
            return;

        cloak.Enabled = !cloak.Enabled;
        Dirty(suitUid, cloak);
    }
    private void OnGetActions(Entity<NinjaCloakComponent> ent, ref GetItemActionsEvent args)
    {
        _actions.AddAction(args.User, ref ent.Comp.ActionEntity, ent.Comp.Action, ent.Owner);
    }

    private void OnUnequipped(Entity<NinjaCloakComponent> ent, ref GotUnequippedEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        ent.Comp.Enabled = false;
        Dirty(ent);
    }
}