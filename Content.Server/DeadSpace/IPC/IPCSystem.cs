// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.IPC.Components;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.DeadSpace.IPC.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Ninja.Components;
using Content.Shared.Ninja.Systems;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Power;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.IPC;

public sealed class IPCSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedBatteryDrainerSystem _batteryDrainer = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IPCComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<IPCComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<IPCComponent, ChangeChargeEvent>(OnBatteryChanged);
        SubscribeLocalEvent<IPCComponent, ToggleDrainActionEvent>(OnToggleAction);
        SubscribeLocalEvent<IPCComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovement);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<IPCComponent, BatteryComponent>();

        while (query.MoveNext(out var uid, out var comp, out var battery))
        {
            if (MathHelper.CloseTo(comp.IdleDrainRate, 0f))
                continue;

            var drain = Math.Min(comp.IdleDrainRate * frameTime, battery.CurrentCharge);
            if (drain > 0f)
                _battery.TryUseCharge(uid, drain, battery);

            var now = _timing.CurTime;
            if (comp.NextBatteryAlertUpdate > now)
                continue;

            comp.NextBatteryAlertUpdate = now + TimeSpan.FromSeconds(1);
            UpdateBatteryAlert(uid, comp, battery);
        }
    }

    private void OnComponentInit(EntityUid uid, IPCComponent comp, ComponentInit args)
    {
        if (TryComp<BatteryComponent>(uid, out var battery))
            UpdateBatteryAlert(uid, comp, battery);

        _actions.AddAction(uid, ref comp.ActionEntity, comp.DrainBatteryAction);

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnComponentShutdown(EntityUid uid, IPCComponent comp, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, comp.ActionEntity);
        RemComp<BatteryDrainerComponent>(uid);
    }

    private void OnBatteryChanged(EntityUid uid, IPCComponent comp, ChangeChargeEvent args)
    {
        if (!TryComp<BatteryComponent>(uid, out var battery))
            return;

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
        {
            RemComp<BatteryDrainerComponent>(uid);
        }
    }

    private void OnRefreshMovement(EntityUid uid, IPCComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<BatteryComponent>(uid, out var battery))
            return;

        var chargePercent = battery.MaxCharge > 0
            ? battery.CurrentCharge / battery.MaxCharge
            : 0f;

        if (chargePercent < comp.BatteryLowThreshold)
            args.ModifySpeed(comp.MovementPenalty);
    }

    private void UpdateBatteryAlert(EntityUid uid, IPCComponent comp, BatteryComponent battery)
    {
        var chargePercent = battery.MaxCharge > 0
            ? battery.CurrentCharge / battery.MaxCharge
            : 0f;

        short newLevel;
        var maxLevels = IPCComponent.MaxBatteryAlertLevels;

        if (battery.CurrentCharge <= 0)
            newLevel = 0;
        else
            newLevel = (short)Math.Clamp(MathF.Ceiling(chargePercent * maxLevels), 1, maxLevels);

        if (comp.LastBatteryLevel != newLevel)
        {
            if (newLevel == 0)
            {
                _alerts.ClearAlert(uid, comp.BatteryAlert);
                _alerts.ShowAlert(uid, comp.NoBatteryAlert);
            }
            else
            {
                _alerts.ClearAlert(uid, comp.NoBatteryAlert);
                _alerts.ShowAlert(uid, comp.BatteryAlert, newLevel);
            }

            comp.LastBatteryLevel = newLevel;
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
        }
    }
}
