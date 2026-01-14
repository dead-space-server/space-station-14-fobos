// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Server.Audio;
using Content.Shared.Examine;
using Robust.Shared.Containers;
using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.DeviceLinking.Events;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared.DeadSpace.Virus.Components;
using Robust.Server.GameObjects;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DeadSpace.Virus;
using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Content.Shared.Body.Prototypes;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusSolutionAnalyzerSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly VirusDiagnoserConsoleSystem _console = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly VirusDiagnoserDataServerSystem _dataServer = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly VirusEvolutionConsoleSystem _evolutionConsoleSystem = default!;
    private const string FlaskContainerKey = "flask_container_virus_solution_analyzer";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusSolutionAnalyzerComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<VirusSolutionAnalyzerComponent, AnchorStateChangedEvent>(OnAnchor);
        SubscribeLocalEvent<VirusSolutionAnalyzerComponent, PortDisconnectedEvent>(OnPortDisconnected);
        SubscribeLocalEvent<VirusSolutionAnalyzerComponent, EntInsertedIntoContainerMessage>(OnEntInsertCont);
        SubscribeLocalEvent<VirusSolutionAnalyzerComponent, EntRemovedFromContainerMessage>(OnEntRemoveCont);
    }

    private void OnEntInsertCont(Entity<VirusSolutionAnalyzerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        UpdateContainerAppearance((ent, ent.Comp));
    }

    private void OnEntRemoveCont(Entity<VirusSolutionAnalyzerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        UpdateContainerAppearance((ent, ent.Comp));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VirusSolutionAnalyzerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_powerReceiverSystem.IsPowered(uid))
            {
                SetStatus((uid, comp), VirusSolutionAnalyzerStatus.Off);
                continue; // без питания ничего не делаем
            }

            // Если был выключен — включаем
            if (comp.Status == VirusSolutionAnalyzerStatus.Off)
                SetStatus((uid, comp), VirusSolutionAnalyzerStatus.On);

            if (EntityManager.EntityExists(comp.CurrentSoundEntity))
                continue;

            switch (comp.Status)
            {
                case VirusSolutionAnalyzerStatus.Scanning:
                    if (!CanScanning((uid, comp)))
                    {
                        SetStatus((uid, comp), VirusSolutionAnalyzerStatus.Denial);
                        break;
                    }

                    EndScanVirus((uid, comp));
                    break;

                case VirusSolutionAnalyzerStatus.Denial:
                    SetStatus((uid, comp), VirusSolutionAnalyzerStatus.On);
                    break;

                case VirusSolutionAnalyzerStatus.Successfully:
                    SetStatus((uid, comp), VirusSolutionAnalyzerStatus.On);
                    break;

                case VirusSolutionAnalyzerStatus.On:
                default:
                    break;
            }

        }
    }

    private void OnPortDisconnected(Entity<VirusSolutionAnalyzerComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port == ent.Comp.VirusSolutionAnalyzerPort)
            ent.Comp.ConnectedConsole = null;
    }

    private void OnAnchor(Entity<VirusSolutionAnalyzerComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (ent.Comp.ConnectedConsole != null && TryComp<VirusDiagnoserConsoleComponent>(ent.Comp.ConnectedConsole, out var console))
        {

            if (args.Anchored)
            {
                _console.RecheckConnections((ent.Comp.ConnectedConsole.Value, console));
                return;
            }

            _console.UpdateUserInterface((ent.Comp.ConnectedConsole.Value, console));
        }

        if (ent.Comp.ConnectedEvolutionConsole != null && TryComp<VirusEvolutionConsoleComponent>(ent.Comp.ConnectedEvolutionConsole, out var evolutionConsole))
        {

            if (args.Anchored)
            {
                _evolutionConsoleSystem.RecheckConnections((ent.Comp.ConnectedEvolutionConsole.Value, evolutionConsole));
                return;
            }

            _evolutionConsoleSystem.UpdateUserInterface((ent.Comp.ConnectedEvolutionConsole.Value, evolutionConsole));
        }
    }

    private void OnExamine(EntityUid uid, VirusSolutionAnalyzerComponent component, ExaminedEvent args)
    {
        BaseContainer? container = default!;

        if (_container.TryGetContainer(uid, FlaskContainerKey, out container))
        {
            if (container is ContainerSlot slot)
            {
                if (slot.ContainedEntity != null)
                    args.PushMarkup(Loc.GetString("virus-diagnoser-flask-attached"));
            }
        }
    }

    public void StartScanVirus(Entity<VirusSolutionAnalyzerComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (!CanScanning((ent, ent.Comp)))
        {
            SetStatus((ent, ent.Comp), VirusSolutionAnalyzerStatus.Denial);
            return;
        }

        SetStatus((ent, ent.Comp), VirusSolutionAnalyzerStatus.Scanning);
    }

    private void EndScanVirus(Entity<VirusSolutionAnalyzerComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        SetStatus((ent, ent.Comp), VirusSolutionAnalyzerStatus.Successfully);

        if (ent.Comp.ConnectedConsole == null ||
            !TryComp<VirusDiagnoserConsoleComponent>(
                ent.Comp.ConnectedConsole,
                out var console))
            return;

        if (!TryGetVirusDataFromContainer(ent, out var virusData))
            return;

        if (!TryComp<VirusDiagnoserDataServerComponent>(
                console.VirusDiagnoserDataServer,
                out var server))
            return;

        foreach (var data in virusData)
        {
            _dataServer.SaveData(
                (console.VirusDiagnoserDataServer.Value, server),
                data);
        }

        _console.UpdateUserInterface(
            (ent.Comp.ConnectedConsole.Value, console));
    }


    public bool TryGetVirusDataFromContainer(
    EntityUid owner,
    out List<VirusData> virusData)
    {
        virusData = new();

        if (!_container.TryGetContainer(owner, FlaskContainerKey, out var container))
            return false;

        if (container is not ContainerSlot slot)
            return false;

        if (slot.ContainedEntity is not { } contained)
            return false;

        if (!TryComp<SolutionContainerManagerComponent>(contained, out var solutionManager))
            return false;

        if (!TryComp<DrawableSolutionComponent>(contained, out var drawable))
            return false;

        var wrapper = new Entity<DrawableSolutionComponent?, SolutionContainerManagerComponent?>(
            contained,
            drawable,
            solutionManager);

        if (!_solutionContainer.TryGetDrawableSolution(
                wrapper,
                out _,
                out var solution))
            return false;

        if (solution == null || solution.Contents.Count == 0)
            return false;

        foreach (var reagent in solution.Contents)
        {
            var dataList = reagent.Reagent.Data;
            if (dataList == null)
                continue;

            foreach (var data in dataList.OfType<VirusData>())
            {
                virusData.Add(data);
            }
        }

        return virusData.Count > 0;
    }

    public void AddSymptom(Entity<VirusSolutionAnalyzerComponent?> console, string symptom)
    {
        if (!Resolve(console, ref console.Comp, false))
            return;

        if (console.Comp.Status != VirusSolutionAnalyzerStatus.On)
            return;

        SetStatus((console, console.Comp), VirusSolutionAnalyzerStatus.Successfully);

        if (_prototypeManager.Index<VirusSymptomPrototype>(symptom) == null)
            return;

        if (!TryGetVirusDataFromContainer(console, out var virusDataList))
            return;

        var virusData = virusDataList.FirstOrDefault();

        if (virusData == null)
            return;

        virusData.ActiveSymptom.Add(symptom);
    }

    public void AddBody(Entity<VirusSolutionAnalyzerComponent?> console, string body)
    {
        if (!Resolve(console, ref console.Comp, false))
            return;

        if (console.Comp.Status != VirusSolutionAnalyzerStatus.On)
            return;

        SetStatus((console, console.Comp), VirusSolutionAnalyzerStatus.Successfully);

        if (_prototypeManager.Index<BodyPrototype>(body) == null)
            return;

        if (!TryGetVirusDataFromContainer(console, out var virusDataList))
            return;

        var virusData = virusDataList.FirstOrDefault();

        if (virusData == null)
            return;

        virusData.BodyWhitelist.Add(body);
    }

    public void RemSymptom(Entity<VirusSolutionAnalyzerComponent?> console, string symptom)
    {
        if (!Resolve(console, ref console.Comp, false))
            return;

        if (console.Comp.Status != VirusSolutionAnalyzerStatus.On)
            return;

        SetStatus((console, console.Comp), VirusSolutionAnalyzerStatus.Successfully);

        if (_prototypeManager.Index<VirusSymptomPrototype>(symptom) == null)
            return;

        if (!TryGetVirusDataFromContainer(console, out var virusDataList))
            return;

        var virusData = virusDataList.FirstOrDefault();

        if (virusData == null)
            return;

        virusData.ActiveSymptom.Remove(symptom);
    }

    public void RemBody(Entity<VirusSolutionAnalyzerComponent?> console, string body)
    {
        if (!Resolve(console, ref console.Comp, false))
            return;

        if (console.Comp.Status != VirusSolutionAnalyzerStatus.On)
            return;

        SetStatus((console, console.Comp), VirusSolutionAnalyzerStatus.Successfully);

        if (_prototypeManager.Index<BodyPrototype>(body) == null)
            return;

        if (!TryGetVirusDataFromContainer(console, out var virusDataList))
            return;

        var virusData = virusDataList.FirstOrDefault();

        if (virusData == null)
            return;

        virusData.BodyWhitelist.Remove(body);
    }

    private void UpdateAppearance(Entity<VirusSolutionAnalyzerComponent> ent)
    {
        if (TryComp<AppearanceComponent>(ent, out var appearance))
            _appearance.SetData(ent, VirusSolutionAnalyzerVisuals.Status, ent.Comp.Status, appearance);
    }

    private void UpdateContainerAppearance(Entity<VirusSolutionAnalyzerComponent> ent)
    {
        if (!TryComp<AppearanceComponent>(ent, out var appearance))
            return;

        if (!_container.TryGetContainer(ent, FlaskContainerKey, out var flaskContainer) ||
            flaskContainer is not ContainerSlot slot ||
            slot.ContainedEntity == null)
        {
            _appearance.SetData(ent, VirusSolutionContainerAnalyzerVisuals.Status, VirusSolutionContainerAnalyzerStatus.Empty, appearance);
            return;
        }

        _appearance.SetData(ent, VirusSolutionContainerAnalyzerVisuals.Status, VirusSolutionContainerAnalyzerStatus.Fill, appearance);
    }


    private void SetStatus(Entity<VirusSolutionAnalyzerComponent?> ent, VirusSolutionAnalyzerStatus newStatus)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (ent.Comp.Status == newStatus)
            return;

        if (newStatus != VirusSolutionAnalyzerStatus.On)
            QueueDel(ent.Comp.CurrentSoundEntity);

        ent.Comp.CurrentSoundEntity = null;

        switch (newStatus)
        {
            case VirusSolutionAnalyzerStatus.On:
                break;
            case VirusSolutionAnalyzerStatus.Off:
                break;
            case VirusSolutionAnalyzerStatus.Scanning:
                ent.Comp.CurrentSoundEntity = _audio.PlayPvs(ent.Comp.ScanningSound, ent)?.Entity;
                break;
            case VirusSolutionAnalyzerStatus.Denial:
                ent.Comp.CurrentSoundEntity = _audio.PlayPvs(ent.Comp.DenialSound, ent)?.Entity;
                break;
            case VirusSolutionAnalyzerStatus.Successfully:
                ent.Comp.CurrentSoundEntity = _audio.PlayPvs(ent.Comp.SuccessfullySound, ent)?.Entity;
                break;
            default:

                break;
        }

        ent.Comp.Status = newStatus;

        UpdateAppearance((ent, ent.Comp));
    }

    public bool CanScanning(Entity<VirusSolutionAnalyzerComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!_container.TryGetContainer(ent, FlaskContainerKey, out var flaskContainer))
            return false;

        if (flaskContainer is not ContainerSlot slot)
            return false;

        if (slot.ContainedEntity == null)
            return false;

        return true;
    }

}