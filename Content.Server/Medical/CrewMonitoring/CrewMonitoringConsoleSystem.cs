using System.Linq;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Station.Systems;
using Content.Shared.PowerCell;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.Medical.CrewMonitoring;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Pinpointer;
using Content.Shared.Silicons.StationAi;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Robust.Shared.Timing;


namespace Content.Server.Medical.CrewMonitoring;

public sealed class CrewMonitoringConsoleSystem : EntitySystem
{

    [Dependency] private readonly SharedPopupSystem _popup = default!;

    [Dependency] private readonly SharedAudioSystem _audio = default!;

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PowerCellSystem _cell = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    // DS14-start
    [Dependency] private readonly SharedStationAiSystem _stationAi = default!;
    [Dependency] private readonly StationSystem _station = default!;
    // DS14-end

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrewMonitoringConsoleComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<CrewMonitoringConsoleComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<CrewMonitoringConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<CrewMonitoringConsoleComponent, GetVerbsEvent<Verb>>(OnGetVerb);
    }

    private void OnRemove(EntityUid uid, CrewMonitoringConsoleComponent component, ComponentRemove args)
    {
        component.ConnectedSensors.Clear();
    }

    private void Bonk(Entity<CrewMonitoringConsoleComponent> ent, CrewMonitoringConsolePingMode pingmode)
    {
        if (!_cell.HasActivatableCharge(ent.Owner))
            return;

        if (ent.Comp.CurrentPingMode == CrewMonitoringConsolePingMode.Disabled)
            return;

        if (ent.Comp.CurrentPingMode > pingmode)
            return;

        var curTime = _timing.CurTime;
        if (ent.Comp.NextSound > curTime)
            return;

        ent.Comp.NextSound = curTime + ent.Comp.Interval;

        _popup.PopupEntity($"{MetaData(ent.Owner).EntityName} Пикает", ent.Owner, PopupType.Medium);
        _audio.PlayPredicted(new SoundPathSpecifier("/Audio/Effects/beep1.ogg"), ent, null);
    }

    private void OnPacketReceived(EntityUid uid, CrewMonitoringConsoleComponent component, DeviceNetworkPacketEvent args)
    {
        var payload = args.Data;

        if (payload.TryGetValue("PingMode", out CrewMonitoringConsolePingMode pingmode))
            Bonk((uid, component), pingmode);

        // Check command
        if (!payload.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return;

        if (command != DeviceNetworkConstants.CmdUpdatedState)
            return;

        if (!payload.TryGetValue(SuitSensorConstants.NET_STATUS_COLLECTION, out Dictionary<string, SuitSensorStatus>? sensorStatus))
            return;

        component.ConnectedSensors = sensorStatus;
        UpdateUserInterface(uid, component);
    }

    private void OnUIOpened(EntityUid uid, CrewMonitoringConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (!_cell.TryUseActivatableCharge(uid))
            return;

        TryUpdateStationAiFallback(uid, component); // DS14
        UpdateUserInterface(uid, component);
    }

    // DS14-start
    private void TryUpdateStationAiFallback(EntityUid uid, CrewMonitoringConsoleComponent component)
    {
        if (component.ConnectedSensors.Count != 0 ||
            !HasComp<StationAiHeldComponent>(uid) ||
            !_stationAi.TryGetCore(uid, out var core) ||
            core.Comp == null ||
            _station.GetOwningStation(core.Owner) is not { } station)
        {
            return;
        }

        var query = EntityQueryEnumerator<CrewMonitoringServerComponent, SingletonDeviceNetServerComponent>();
        while (query.MoveNext(out var serverUid, out var server, out var singleton))
        {
            if (!singleton.Active ||
                _station.GetOwningStation(serverUid) != station)
                continue;

            component.ConnectedSensors = new Dictionary<string, SuitSensorStatus>(server.SensorStatus);
            return;
        }
    }
    // DS14-end

    private void UpdateUserInterface(EntityUid uid, CrewMonitoringConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_uiSystem.IsUiOpen(uid, CrewMonitoringUIKey.Key))
            return;

        // The grid must have a NavMapComponent to visualize the map in the UI
        var xform = Transform(uid);

        if (xform.GridUid != null)
            EnsureComp<NavMapComponent>(xform.GridUid.Value);

        // Update all sensors info
        var allSensors = component.ConnectedSensors.Values.ToList();
        _uiSystem.SetUiState(uid, CrewMonitoringUIKey.Key, new CrewMonitoringState(allSensors));
    }

    private void OnGetVerb(EntityUid uid, CrewMonitoringConsoleComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !args.CanComplexInteract)
            return;
        for (var i = 0; i < component.PingModes.Count; i++)
        {

            var pingmode = component.PingModes[i];
            var icon = GetSpriteByMode(pingmode);
            var text = GetTextByMode(pingmode);

            var v = new Verb
            {
                Priority = component.PingModes.Count - i + 1,
                Icon = icon,
                Disabled = pingmode == component.CurrentPingMode,
                Category = VerbCategory.PingSelect,
                Text = text,
                Impact = LogImpact.Low,
                DoContactInteraction = false,
                CloseMenu = true,
                Act = () =>
                {
                    _popup.PopupEntity("Измененно на " + text, uid, args.User);
                    component.CurrentPingMode = pingmode;
                }
            };

            args.Verbs.Add(v);
        }

        var d = new Verb
        {
            Priority = 1,
            Category = VerbCategory.PingSelect,
            Text = "Выключить",
            Impact = LogImpact.Low,
            Disabled = component.CurrentPingMode == CrewMonitoringConsolePingMode.Disabled,
            DoContactInteraction = false,
            CloseMenu = true,
            Act = () =>
            {
                _popup.PopupEntity("Выключенно", uid, args.User);
                component.CurrentPingMode = CrewMonitoringConsolePingMode.Disabled;
            }
        };
        args.Verbs.Add(d);
    }

    public SpriteSpecifier? GetSpriteByMode(CrewMonitoringConsolePingMode mode)
    {
        switch (mode)
        {
            case CrewMonitoringConsolePingMode.Health4:
                return new SpriteSpecifier.Rsi(new ResPath("Interface/Alerts/human_crew_monitoring.rsi"), "health4");

            case CrewMonitoringConsolePingMode.Krit:
                return new SpriteSpecifier.Rsi(new ResPath("Interface/Alerts/human_crew_monitoring.rsi"), "critical");

            case CrewMonitoringConsolePingMode.Dead:
                return new SpriteSpecifier.Rsi(new ResPath("Interface/Alerts/human_crew_monitoring.rsi"), "dead");
        }

        return null;
    }

    public string GetTextByMode(CrewMonitoringConsolePingMode mode)
    {
        switch (mode)
        {
            case CrewMonitoringConsolePingMode.Health4:
                return "Ужс";

            case CrewMonitoringConsolePingMode.Krit:
                return "Крит";

            case CrewMonitoringConsolePingMode.Dead:
                return "Труп";

            case CrewMonitoringConsolePingMode.Disabled:
                return "Выключить";
        }

        return "Ошибка";
    }
}
