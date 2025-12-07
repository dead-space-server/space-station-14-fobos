using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.DeadSpace.IPC;
using Content.Shared.Emp;
using Content.Shared.Movement.Systems;
using Content.Shared.Ninja.Components;
using Content.Shared.Ninja.Systems;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;

namespace Content.Server.DeadSpace.IPC;

public sealed class IPCSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedBatteryDrainerSystem _batteryDrainer = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    private const float BatteryLowThreshold = 0.01f;
    private const float MovementPenalty = 0.2f;
    private const short BatteryAlertLevels = 10;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IPCComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<IPCComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<IPCComponent, ChangeChargeEvent>(OnBatteryChanged);
        SubscribeLocalEvent<IPCComponent, ToggleDrainActionEvent>(OnToggleAction);
        SubscribeLocalEvent<IPCComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<IPCComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovement);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<IPCComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (!TryComp<BatteryComponent>(uid, out var battery))
                continue;

            var drain = comp.IdleDrainRate * frameTime;
            if (drain <= 0)
                continue;

            _battery.TryUseCharge(uid, drain, battery);

            UpdateBatteryAlert(uid, comp);
        }
    }

    private void OnMapInit(EntityUid uid, IPCComponent comp, MapInitEvent args)
    {
        UpdateBatteryAlert(uid, comp);

        if (TryComp<ActionsComponent>(uid, out _))
        {
            comp.ActionEntity = null;
            _action.AddAction(uid, ref comp.ActionEntity, comp.DrainBatteryAction);
        }

        _movement.RefreshMovementSpeedModifiers(uid);
    }

    private void OnComponentShutdown(EntityUid uid, IPCComponent comp, ComponentShutdown args)
    {
        _action.RemoveAction(uid, comp.ActionEntity);
        RemComp<BatteryDrainerComponent>(uid);
    }

    private void OnBatteryChanged(EntityUid uid, IPCComponent comp, ChangeChargeEvent args)
    {
        if (MetaData(uid).EntityLifeStage < EntityLifeStage.Terminating)
            UpdateBatteryAlert(uid, comp);
    }

    private void OnToggleAction(EntityUid uid, IPCComponent comp, ToggleDrainActionEvent args)
    {
        if (args.Handled)
            return;

        comp.DrainActivated = !comp.DrainActivated;
        _action.SetToggled(comp.ActionEntity, comp.DrainActivated);
        args.Handled = true;

        if (comp.DrainActivated && TryComp<BatteryComponent>(uid, out var battery))
        {
            EnsureComp<BatteryDrainerComponent>(uid);
            _batteryDrainer.SetBattery(uid, uid);
        }
        else
            RemComp<BatteryDrainerComponent>(uid);
    }

    private void UpdateBatteryAlert(EntityUid uid, IPCComponent comp)
    {
        if (!TryComp<BatteryComponent>(uid, out var battery) ||
            battery.MaxCharge <= 0 ||
            battery.CurrentCharge / battery.MaxCharge < BatteryLowThreshold)
        {
            ShowNoBattery(uid, comp);
            return;
        }

        var level = (short)Math.Clamp(
            MathF.Round(battery.CurrentCharge / battery.MaxCharge * BatteryAlertLevels),
            1,
            BatteryAlertLevels);

        _alerts.ClearAlert(uid, comp.NoBatteryAlert);
        _alerts.ShowAlert(uid, comp.BatteryAlert, level);
        _movement.RefreshMovementSpeedModifiers(uid);
    }

    private void ShowNoBattery(EntityUid uid, IPCComponent comp)
    {
        _alerts.ClearAlert(uid, comp.BatteryAlert);
        _alerts.ShowAlert(uid, comp.NoBatteryAlert);
        _movement.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRefreshMovement(EntityUid uid, IPCComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<BatteryComponent>(uid, out var battery) ||
            battery.MaxCharge <= 0 ||
            battery.CurrentCharge / battery.MaxCharge < BatteryLowThreshold)
            args.ModifySpeed(MovementPenalty);
    }

    private void OnEmpPulse(EntityUid uid, IPCComponent comp, ref EmpPulseEvent args)
    {
        args.Affected = true;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Shock", comp.EmpDamage);
        _damageable.TryChangeDamage(uid, damage);
    }
}
