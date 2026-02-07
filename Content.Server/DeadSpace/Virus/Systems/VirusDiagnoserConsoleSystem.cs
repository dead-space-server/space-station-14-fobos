// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared.UserInterface;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Content.Shared.DeadSpace.Virus;
using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.Virus.Components;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusDiagnoserConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly VirusDiagnoserDataServerSystem _dataServer = default!;
    [Dependency] private readonly VirusDiagnoserSystem _diagnoser = default!;
    [Dependency] private readonly VirusSolutionAnalyzerSystem _virusSolutionAnalyzer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusDiagnoserConsoleComponent, UiButtonPressedMessage>(OnButtonPressed);
        SubscribeLocalEvent<VirusDiagnoserConsoleComponent, AfterActivatableUIOpenEvent>(OnUIOpen);
        SubscribeLocalEvent<VirusDiagnoserConsoleComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<VirusDiagnoserConsoleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<VirusDiagnoserConsoleComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<VirusDiagnoserConsoleComponent, PortDisconnectedEvent>(OnPortDisconnected);
        SubscribeLocalEvent<VirusDiagnoserConsoleComponent, AnchorStateChangedEvent>(OnAnchorChanged);
    }

    private void OnButtonPressed(EntityUid uid, VirusDiagnoserConsoleComponent component, UiButtonPressedMessage args)
    {
        if (!_powerReceiverSystem.IsPowered(uid))
            return;

        var dataServer = new VirusDiagnoserDataServerComponent();
        var diagnoser = new VirusDiagnoserComponent();

        switch (args.Button)
        {
            case UiButton.StartAnalys:
                {
                    if (component.VirusSolutionAnalyzer == null || !TryComp<VirusSolutionAnalyzerComponent>(component.VirusSolutionAnalyzer, out var virusSolutionAnalyzer))
                        return;

                    _virusSolutionAnalyzer.StartScanVirus((component.VirusSolutionAnalyzer.Value, virusSolutionAnalyzer));
                    break;
                }
            case UiButton.DeleteData:
                {
                    if (component.VirusDiagnoserDataServer == null || !TryComp(component.VirusDiagnoserDataServer, out dataServer))
                        return;

                    if (string.IsNullOrEmpty(args.Strain))
                        return;

                    _dataServer.DeleteData((component.VirusDiagnoserDataServer.Value, dataServer), args.Strain);
                    break;
                }
            case UiButton.GenerateVirus:
                {
                    if (component.VirusDiagnoser == null || !TryComp(component.VirusDiagnoser, out diagnoser))
                        return;

                    if (component.VirusDiagnoserDataServer == null || !TryComp(component.VirusDiagnoserDataServer, out dataServer))
                        return;

                    if (string.IsNullOrEmpty(args.Strain))
                        return;

                    VirusData? data = _dataServer.GetData((component.VirusDiagnoserDataServer.Value, dataServer), args.Strain);

                    _diagnoser.StartGenerateVirus((component.VirusDiagnoser.Value, diagnoser), data);
                    break;
                }
            case UiButton.PrintReport:
                {
                    if (component.VirusDiagnoser == null || !TryComp(component.VirusDiagnoser, out diagnoser))
                        return;

                    if (component.VirusDiagnoserDataServer == null || !TryComp(component.VirusDiagnoserDataServer, out dataServer))
                        return;

                    if (String.IsNullOrEmpty(args.Strain))
                        return;

                    VirusData? data = _dataServer.GetData((component.VirusDiagnoserDataServer.Value, dataServer), args.Strain);

                    _diagnoser.StartPrinting((component.VirusDiagnoser.Value, diagnoser), data);
                    break;
                }
            case UiButton.ScanVirus:
                {
                    if (component.VirusDiagnoser == null || !TryComp(component.VirusDiagnoser, out diagnoser))
                        return;

                    _diagnoser.StartScanVirus((component.VirusDiagnoser.Value, diagnoser));
                    break;
                }
            default:
                break;
        }
        UpdateUserInterface((uid, component));
    }

    private void OnPowerChanged(EntityUid uid, VirusDiagnoserConsoleComponent component, ref PowerChangedEvent args)
    {
        RecheckConnections((uid, component));
    }

    private void OnMapInit(EntityUid uid, VirusDiagnoserConsoleComponent component, MapInitEvent args)
    {
        if (!TryComp<DeviceLinkSourceComponent>(uid, out var receiver))
            return;

        foreach (var port in receiver.Outputs.Values.SelectMany(ports => ports))
        {
            if (TryComp<VirusDiagnoserComponent>(port, out var diagnoser))
            {
                component.VirusDiagnoser = port;
                diagnoser.ConnectedConsole = uid;
            }

            if (TryComp<VirusDiagnoserDataServerComponent>(port, out var server))
            {
                component.VirusDiagnoserDataServer = port;
                server.ConnectedConsole = uid;
            }

            if (TryComp<VirusSolutionAnalyzerComponent>(port, out var solutionAnalyzer))
            {
                component.VirusSolutionAnalyzer = port;
                solutionAnalyzer.ConnectedConsole = uid;
            }
        }
    }

    private void OnNewLink(EntityUid uid, VirusDiagnoserConsoleComponent component, NewLinkEvent args)
    {
        if (TryComp<VirusDiagnoserComponent>(args.Sink, out var diagnoser) && args.SourcePort == component.VirusDiagnoserPort)
        {
            component.VirusDiagnoser = args.Sink;
            diagnoser.ConnectedConsole = uid;
        }

        if (TryComp<VirusDiagnoserDataServerComponent>(args.Sink, out var server) && args.SourcePort == component.VirusDiagnoserDataServerPort)
        {
            component.VirusDiagnoserDataServer = args.Sink;
            server.ConnectedConsole = uid;
        }

        if (TryComp<VirusSolutionAnalyzerComponent>(args.Sink, out var solutionAnalyzer) && args.SourcePort == component.VirusSolutionAnalyzerPort)
        {
            component.VirusSolutionAnalyzer = args.Sink;
            solutionAnalyzer.ConnectedConsole = uid;
        }

        RecheckConnections((uid, component));
    }

    private void OnPortDisconnected(Entity<VirusDiagnoserConsoleComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port == ent.Comp.VirusDiagnoserPort)
            ent.Comp.VirusDiagnoser = null;

        if (args.Port == ent.Comp.VirusSolutionAnalyzerPort)
            ent.Comp.VirusSolutionAnalyzer = null;

        if (args.Port == ent.Comp.VirusDiagnoserDataServerPort)
            ent.Comp.VirusDiagnoserDataServer = null;

        UpdateUserInterface((ent, ent.Comp));
    }

    private void OnUIOpen(EntityUid uid, VirusDiagnoserConsoleComponent component, AfterActivatableUIOpenEvent args)
    {
        RecheckConnections((uid, component));
    }

    private void OnAnchorChanged(EntityUid uid, VirusDiagnoserConsoleComponent component, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
        {
            RecheckConnections((uid, component));
            return;
        }

        RecheckConnections((uid, component));
    }

    public void UpdateUserInterface(Entity<VirusDiagnoserConsoleComponent?> console)
    {
        if (!Resolve(console, ref console.Comp, false))
            return;

        if (!TryComp<UserInterfaceComponent>(console, out var userInterface))
            return;

        if (!_uiSystem.HasUi(console, VirusDiagnoserConsoleUiKey.Key, userInterface))
            return;

        if (!_powerReceiverSystem.IsPowered(console))
        {
            _uiSystem.CloseUis((console, userInterface));
            return;
        }

        var newState = GetUserInterfaceState((console, console.Comp));
        _uiSystem.SetUiState((console, userInterface), VirusDiagnoserConsoleUiKey.Key, newState);
    }

    public void RecheckConnections(Entity<VirusDiagnoserConsoleComponent?> console)
    {
        if (!Resolve(console, ref console.Comp, false))
            return;

        var distance = 0f;

        if (console.Comp.VirusDiagnoser != null)
        {
            Transform(console.Comp.VirusDiagnoser.Value).Coordinates.TryDistance(EntityManager, Transform(console).Coordinates, out distance);
            console.Comp.DiagnoserInRange = distance <= console.Comp.MaxDistanceForOther;
        }
        if (console.Comp.VirusDiagnoserDataServer != null)
        {
            Transform(console.Comp.VirusDiagnoserDataServer.Value).Coordinates.TryDistance(EntityManager, Transform(console).Coordinates, out distance);
            console.Comp.DataServerInRange = distance <= console.Comp.MaxDistanceForDataServer;
        }
        if (console.Comp.VirusSolutionAnalyzer != null)
        {
            Transform(console.Comp.VirusSolutionAnalyzer.Value).Coordinates.TryDistance(EntityManager, Transform(console).Coordinates, out distance);
            console.Comp.SolutionAnalyzerInRange = distance <= console.Comp.MaxDistanceForOther;
        }

        UpdateUserInterface((console, console.Comp));
    }

    private VirusDiagnoserConsoleBoundUserInterfaceState GetUserInterfaceState(Entity<VirusDiagnoserConsoleComponent?> console)
    {
        if (!Resolve(console, ref console.Comp, false))
            return default!;

        VirusDiagnoserDataServerComponent? dataServer = null;

        List<VirusStrainRecord> strains;

        if (console.Comp.VirusDiagnoserDataServer != null &&
            TryComp(console.Comp.VirusDiagnoserDataServer, out dataServer))
        {
            strains = _dataServer.GetAllStrains((console.Comp.VirusDiagnoserDataServer.Value, dataServer));
        }
        else
        {
            strains = new List<VirusStrainRecord>();
        }

        var points = dataServer?.Points ?? 0;

        var diagnoserConnected = console.Comp.VirusDiagnoser != null;
        var dataServerConnected = console.Comp.VirusDiagnoserDataServer != null;
        var solutionAnalyzerConnected = console.Comp.VirusSolutionAnalyzer != null;

        return new VirusDiagnoserConsoleBoundUserInterfaceState(
            strains,
            points,
            diagnoserConnected,
            dataServerConnected,
            solutionAnalyzerConnected,
            console.Comp.DiagnoserInRange,
            console.Comp.DataServerInRange,
            console.Comp.SolutionAnalyzerInRange
        );
    }


}

