using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Alert;
using Content.Shared.DeadSpace.IPC;
using Content.Shared.Movement.Systems;
using Content.Shared.Ninja.Components;
using Content.Shared.Ninja.Systems;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Power;

namespace Content.Server.DeadSpace.IPC;

public sealed class IPCSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedBatteryDrainerSystem _batteryDrainer = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IPCComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<IPCComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<IPCComponent, ChangeChargeEvent>(OnBatteryChanged);
        SubscribeLocalEvent<IPCComponent, ToggleDrainActionEvent>(OnToggleAction);
        SubscribeLocalEvent<IPCComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovement);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<IPCComponent, BatteryComponent>();

        while (query.MoveNext(out var uid, out var comp, out var battery))
        {
            if (MathHelper.CloseTo(comp.IdleDrainRate, 0f))
                continue;

            var drain = comp.IdleDrainRate * frameTime;
            if (drain <= 0)
                continue;

            _battery.TryUseCharge(uid, drain, battery);
            UpdateBatteryAlert(uid, comp, battery);
        }
    }

    private void OnMapInit(EntityUid uid, IPCComponent comp, MapInitEvent args)
    {
        if (TryComp<BatteryComponent>(uid, out var battery))
            UpdateBatteryAlert(uid, comp, battery);

        if (HasComp<ActionsComponent>(uid))
        {
            _actions.AddAction(uid, ref comp.ActionEntity, comp.DrainBatteryAction);
        }

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnComponentShutdown(EntityUid uid, IPCComponent comp, ComponentShutdown args)
    {
        if (comp.ActionEntity.HasValue)
        {
            _actions.RemoveAction(uid, comp.ActionEntity.Value);
        }
        RemComp<BatteryDrainerComponent>(uid);
    }

    private void OnBatteryChanged(EntityUid uid, IPCComponent comp, ChangeChargeEvent args)
    {
        if (TryComp<BatteryComponent>(uid, out var battery))
            UpdateBatteryAlert(uid, comp, battery);
    }

    private void OnToggleAction(EntityUid uid, IPCComponent comp, ToggleDrainActionEvent args)
    {
        if (args.Handled)
            return;

        SetDrainActivated(uid, comp, !comp.DrainActivated);
        args.Handled = true;
    }

    private void SetDrainActivated(EntityUid uid, IPCComponent comp, bool value)
    {
        comp.DrainActivated = value;
        _actions.SetToggled(comp.ActionEntity, value);

        if (value && TryComp<BatteryComponent>(uid, out _))
        {
            EnsureComp<BatteryDrainerComponent>(uid);
            _batteryDrainer.SetBattery(uid, uid);
        }
        else
            RemComp<BatteryDrainerComponent>(uid);
    }

    private void OnRefreshMovement(EntityUid uid, IPCComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<BatteryComponent>(uid, out var battery))
            return;

        var chargePercent = battery.CurrentCharge / battery.MaxCharge;

        if (chargePercent < comp.BatteryLowThreshold)
            args.ModifySpeed(comp.MovementPenalty);
    }

    private void UpdateBatteryAlert(EntityUid uid, IPCComponent comp, BatteryComponent battery)
    {
        var chargePercent = battery.CurrentCharge / battery.MaxCharge;
        short newLevel;
        var maxLevels = IPCComponent.MaxBatteryAlertLevels;

        if (battery.MaxCharge <= 0 || chargePercent < comp.BatteryLowThreshold)
        {
            _alerts.ClearAlert(uid, comp.BatteryAlert);
            _alerts.ShowAlert(uid, comp.NoBatteryAlert);
            newLevel = 0;
        }
        else
        {
            newLevel = (short)Math.Clamp(MathF.Round(chargePercent * maxLevels), 1, maxLevels);

            if (comp.LastBatteryLevel != newLevel)
            {
                _alerts.ClearAlert(uid, comp.NoBatteryAlert);
                _alerts.ShowAlert(uid, comp.BatteryAlert, newLevel);
            }
        }

        if (comp.LastBatteryLevel == newLevel)
            return;

        comp.LastBatteryLevel = newLevel;
        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }
}
