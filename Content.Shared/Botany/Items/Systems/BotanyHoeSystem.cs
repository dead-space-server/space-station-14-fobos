using Content.Shared.Botany.Components;
using Content.Shared.Botany.Events;
using Content.Shared.Botany.Items.Components;
using Content.Shared.Botany.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Shared.Botany.Items.Systems;

/// <summary>
/// System for taking a sample of a plant.
/// </summary>
public sealed partial class BotanyHoeSystem : EntitySystem
{
    // DS14-start: current engine uses explicit event subscriptions.
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BotanyHoeComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PlantTrayComponent, TrayHoeAttemptEvent>(OnTrayHoeAttempt);
    }
    // DS14-end

    // DS14-start: current engine uses readonly IoC fields.
    [Dependency] private readonly PlantTraySystem _plantTray = default!;
    [Dependency] private readonly PlantSystem _plant = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    // DS14-end

    // DS14-start: current engine uses readonly IoC fields.
    [Dependency] private readonly EntityQuery<PlantComponent> _plantQuery = default!;
    // DS14-end

    private void OnAfterInteract(Entity<BotanyHoeComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || args.Handled || !args.CanReach)
            return;

        // Allow interacting with either the plant or the tray.
        var target = args.Target.Value;
        if (_plantQuery.TryComp(target, out var targetPlant))
        {
            if (!_plant.TryGetTray((target, targetPlant), out var tray))
                return;

            target = tray.Owner;
        }
        else if (!HasComp<PlantTrayComponent>(target))
            return;

        var ev = new TrayHoeAttemptEvent(ent, args.User);
        RaiseLocalEvent(target, ref ev);

        args.Handled = true;
    }

    private void OnTrayHoeAttempt(Entity<PlantTrayComponent> ent, ref TrayHoeAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (ent.Comp.WeedLevel <= 0)
        {
            _popup.PopupCursor(Loc.GetString("plant-hoe-component-no-weeds-popup"), args.User);
            return;
        }

        _popup.PopupCursor(
            Loc.GetString("plant-hoe-component-already-seeded-popup",
                ("name", ent.Owner)),
            args.User,
            PopupType.Medium);
        _popup.PopupEntity(
            Loc.GetString("plant-hoe-component-remove-weeds-others-popup",
                ("otherName", Identity.Entity(args.User, EntityManager))),
            ent.Owner,
            Filter.PvsExcept(args.User),
            true);

        _plantTray.AdjustWeed(ent.AsNullable(), -args.Hoe.Comp.WeedAmount);
    }
}
