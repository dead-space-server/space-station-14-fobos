using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Cloning.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Medical.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.UserInterface;
using Content.Shared.Cloning;
using Content.Shared.Cloning.CloningConsole;
using Content.Shared.Database;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Robust.Server.Player;

namespace Content.Server.DeadSpace.Virus.Systems;
{
    public sealed class VirusDiagnoserConsoleSystem : EntitySystem
    {
        [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly CloningPodSystem _cloningPodSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;

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
        private void OnButtonPressed(EntityUid uid, VirusDiagnoserConsoleComponent consoleComponent, UiButtonPressedMessage args)
        {
            if (!_powerReceiverSystem.IsPowered(uid))
                return;

            switch (args.Button)
            {
                case UiButton.Clone:
                    if (consoleComponent.GeneticScanner != null && consoleComponent.CloningPod != null)
                        TryClone(uid, consoleComponent.CloningPod.Value, consoleComponent.GeneticScanner.Value, consoleComponent: consoleComponent);
                    break;
            }
            UpdateUserInterface(uid, consoleComponent);
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
                if (TryComp<VirusDiagnoserComponent>(port, out var scanner))
                {
                    component.VirusDiagnoser = port;
                    scanner.ConnectedConsole = uid;
                }

                if (TryComp<VirusDiagnoserDataServerComponent>(port, out var pod))
                {
                    component.VirusDiagnoserDataServer = port;
                    pod.ConnectedConsole = uid;
                }
            }
        }

        private void OnNewLink(EntityUid uid, VirusDiagnoserConsoleComponent component, NewLinkEvent args)
        {
            if (TryComp<VirusDiagnoserComponent>(args.Sink, out var scanner) && args.SourcePort == component.ScannerPort)
            {
                component.VirusDiagnoser = args.Sink;
                scanner.ConnectedConsole = uid;
            }

            if (TryComp<VirusDiagnoserDataServerComponent>(args.Sink, out var pod) && args.SourcePort == component.PodPort)
            {
                component.VirusDiagnoserDataServer = args.Sink;
                pod.ConnectedConsole = uid;
            }
            RecheckConnections((uid, component));
        }

        private void OnPortDisconnected(Entity<VirusDiagnoserConsoleComponent> ent, ref PortDisconnectedEvent args)
        {
            if (args.Port == ent.Comp.VirusDiagnoserPort)
                ent.Comp.VirusDiagnoser = null;

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
            if (!Resolve(entity, ref entity.Comp, false))
               return;

            if (!_uiSystem.HasUi(console, VirusDiagnoserConsole.Key))
                return;

            if (!_powerReceiverSystem.IsPowered(console))
            {
                _uiSystem.CloseUis(console);
                return;
            }

            var newState = GetUserInterfaceState(consoleComponent);
            _uiSystem.SetUiState(console, VirusDiagnoserConsole.Key, newState);
        }

        public void RecheckConnections(Entity<VirusDiagnoserConsoleComponent?> console)
        {
            if (!Resolve(entity, ref entity.Comp, false))
               return;
            
            var distance = 0f;

            if (entity.Comp.VirusDiagnoser != null)
            {
                Transform(entity.Comp.VirusDiagnoser.Value).Coordinates.TryDistance(EntityManager, Transform((console)).Coordinates, out distance);
                consoleComp.DiagnoserInRange = distance <= entity.Comp.MaxDistanceForDiagnoser;
            }
            if (entity.Comp.VirusDiagnoserDataServer != null)
            {
                Transform(entity.Comp.VirusDiagnoserDataServer.Value).Coordinates.TryDistance(EntityManager, Transform((console)).Coordinates, out distance);
                consoleComp.DataServerInRange = distance <= entity.Comp.MaxDistanceForDataServer;
            }

            UpdateUserInterface(console, consoleComp);
        }

        private CloningConsoleBoundUserInterfaceState GetUserInterfaceState(Entity<VirusDiagnoserConsoleComponent?> console)
        {
            if (!Resolve(entity, ref entity.Comp, false))
               return default!;

            ClonerStatus clonerStatus = ClonerStatus.Ready;

            // genetic scanner info
            string scanBodyInfo = Loc.GetString("generic-unknown");
            bool scannerConnected = false;
            bool scannerInRange = consoleComponent.GeneticScannerInRange;
            if (consoleComponent.GeneticScanner != null && TryComp<MedicalScannerComponent>(consoleComponent.GeneticScanner, out var scanner))
            {
                scannerConnected = true;
                EntityUid? scanBody = scanner.BodyContainer.ContainedEntity;

                // GET STATE
                if (scanBody == null || !HasComp<MobStateComponent>(scanBody))
                    clonerStatus = ClonerStatus.ScannerEmpty;
                else
                {
                    scanBodyInfo = MetaData(scanBody.Value).EntityName;

                    if (!_mobStateSystem.IsDead(scanBody.Value))
                    {
                        clonerStatus = ClonerStatus.ScannerOccupantAlive;
                    }
                    else
                    {
                        if (!_mindSystem.TryGetMind(scanBody.Value, out _, out var mind) ||
                            mind.UserId == null ||
                            !_playerManager.TryGetSessionById(mind.UserId.Value, out _))
                        {
                            clonerStatus = ClonerStatus.NoMindDetected;
                        }
                    }
                }
            }

            // cloning pod info
            var cloneBodyInfo = Loc.GetString("generic-unknown");
            bool clonerConnected = false;
            bool clonerMindPresent = false;
            bool clonerInRange = consoleComponent.CloningPodInRange;
            if (consoleComponent.CloningPod != null && TryComp<CloningPodComponent>(consoleComponent.CloningPod, out var clonePod)
            && Transform(consoleComponent.CloningPod.Value).Anchored)
            {
                clonerConnected = true;
                EntityUid? cloneBody = clonePod.BodyContainer.ContainedEntity;

                clonerMindPresent = clonePod.Status == CloningPodStatus.Cloning;
                if (HasComp<ActiveCloningPodComponent>(consoleComponent.CloningPod))
                {
                    if (cloneBody != null)
                        cloneBodyInfo = Identity.Name(cloneBody.Value, EntityManager);
                    clonerStatus = ClonerStatus.ClonerOccupied;
                }
            }
            else
            {
                clonerStatus = ClonerStatus.NoClonerDetected;
            }

            return new CloningConsoleBoundUserInterfaceState(
                scanBodyInfo,
                cloneBodyInfo,
                clonerMindPresent,
                clonerStatus,
                scannerConnected,
                scannerInRange,
                clonerConnected,
                clonerInRange
                );
        }

    }
}
