using System.Numerics;
using Content.Server.UserInterface;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.PowerCell;
using Content.Shared.Movement.Components;
using Content.Shared.DeadSpace.Shuttles.Events;
using Content.Shared.Actions;
using Content.Shared.UserInterface;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Inventory.VirtualItem;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Shuttles.Systems;

public sealed class RadarConsoleSystem : SharedRadarConsoleSystem
{
    [Dependency] private readonly ShuttleConsoleSystem _console = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    // DS14-start
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly RadarBlipSystem _radarBlips = default!;

    private float _updateAccumulator;
    private const float UpdateInterval = 0.5f;
    // DS14-end

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadarConsoleComponent, ComponentStartup>(OnRadarStartup);
        // DS14-start
        SubscribeLocalEvent<RadarConsoleComponent, GotEquippedHandEvent>(OnRadarEquippedHand);
        SubscribeLocalEvent<RadarConsoleComponent, GotUnequippedHandEvent>(OnRadarUnequippedHand);
        SubscribeLocalEvent<RadarConsoleComponent, GotEquippedEvent>(OnRadarEquipped);
        SubscribeLocalEvent<RadarConsoleComponent, GotUnequippedEvent>(OnRadarUnequipped);
        SubscribeLocalEvent<RadarConsoleComponent, ComponentShutdown>(OnRadarConsoleShutdown);
        SubscribeLocalEvent<ToggleHandheldRadarUIEvent>(OnToggleHandheldRadarUI);
        // DS14-end
    }

    private void OnRadarStartup(EntityUid uid, RadarConsoleComponent component, ComponentStartup args)
    {
        UpdateState(uid, component);
    }

    // DS14-start
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateAccumulator += frameTime;
        if (_updateAccumulator < UpdateInterval)
            return;

        _updateAccumulator = 0f;

        var query = EntityQueryEnumerator<RadarConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_uiSystem.IsUiOpen(uid, RadarConsoleUiKey.Key))
                continue;

            UpdateState(uid, comp);
        }
    }
    // DS14-end

    protected override void UpdateState(EntityUid uid, RadarConsoleComponent component)
    {
        var xform = Transform(uid);
        var onGrid = xform.ParentUid == xform.GridUid;
        EntityCoordinates? coordinates = onGrid ? xform.Coordinates : null;
        Angle? angle = onGrid ? xform.LocalRotation : null;

        if (component.FollowEntity)
        {
            coordinates = new EntityCoordinates(uid, Vector2.Zero);
            angle = Angle.Zero;
        }

        // DS14-start
        if (!_uiSystem.HasUi(uid, RadarConsoleUiKey.Key))
            return;

        NavInterfaceState state;
        var docks = _console.GetAllDocks();

        if (coordinates != null && angle != null)
            state = _console.GetNavState(uid, docks, coordinates.Value, angle.Value);
        else
            state = _console.GetNavState(uid, docks);

        state.RotateWithEntity = onGrid && !component.FollowEntity;

        if (component.Advanced)
            state.Blips = _radarBlips.CollectSpaceBlips(uid, component, component.MaxRange); // DS14

        _uiSystem.SetUiState(uid, RadarConsoleUiKey.Key, new NavBoundUserInterfaceState(state));
    }

    private void OnRadarEquippedHand(EntityUid uid, RadarConsoleComponent component, GotEquippedHandEvent args)
    {
        if (component.ToggleAction == null)
            return;

        _actions.AddAction(args.User, ref component.ToggleActionEntity, component.ToggleAction, uid);
    }

    private void OnRadarUnequippedHand(EntityUid uid, RadarConsoleComponent component, GotUnequippedHandEvent args)
    {
        _actions.RemoveAction(args.User, component.ToggleActionEntity);
        component.FollowEntity = false;

        if (_uiSystem.IsUiOpen(uid, RadarConsoleUiKey.Key, args.User))
            _uiSystem.CloseUi(uid, RadarConsoleUiKey.Key, args.User);
    }

    private void OnRadarEquipped(EntityUid uid, RadarConsoleComponent component, GotEquippedEvent args)
    {
        component.FollowEntity = true;

        if (component.ToggleAction == null)
            return;

        _actions.AddAction(args.Equipee, ref component.ToggleActionEntity, component.ToggleAction, uid);
    }

    private void OnRadarUnequipped(EntityUid uid, RadarConsoleComponent component, GotUnequippedEvent args)
    {
        component.FollowEntity = false;

        _actions.RemoveAction(args.Equipee, component.ToggleActionEntity);

        if (_uiSystem.IsUiOpen(uid, RadarConsoleUiKey.Key, args.Equipee))
            _uiSystem.CloseUi(uid, RadarConsoleUiKey.Key, args.Equipee);
    }

    private void OnRadarConsoleShutdown(EntityUid uid, RadarConsoleComponent component, ComponentShutdown args)
    {
        _radarBlips.ClearCache(component); // DS14

        if (component.ToggleActionEntity == null)
            return;

        _actions.RemoveAction(component.ToggleActionEntity.Value);
    }

    private void OnToggleHandheldRadarUI(ToggleHandheldRadarUIEvent args)
    {
        var user = args.Performer;

        foreach (var held in _handsSystem.EnumerateHeld(user))
        {
            if (!TryComp<RadarConsoleComponent>(held, out _))
                continue;

            if (!_uiSystem.HasUi(held, RadarConsoleUiKey.Key))
                continue;

            _uiSystem.TryToggleUi(held, RadarConsoleUiKey.Key, user);
            args.Handled = true;
            return;
        }

        var inventoryQuery = EntityQueryEnumerator<RadarConsoleComponent, TransformComponent>();
        while (inventoryQuery.MoveNext(out var itemUid, out _, out var itemXform))
        {
            if (itemXform.ParentUid != user)
                continue;

            if (!_uiSystem.HasUi(itemUid, RadarConsoleUiKey.Key))
                continue;

            _uiSystem.TryToggleUi(itemUid, RadarConsoleUiKey.Key, user);
            args.Handled = true;
            return;
        }
    }
    // DS14-end
}
