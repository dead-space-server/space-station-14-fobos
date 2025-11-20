// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Server.Audio;
using Content.Shared.Examine;
using Robust.Shared.Containers;
using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Paper;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.Virus.Components;
using Robust.Server.GameObjects;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Virus;
using Content.Shared.DeadSpace.Virus.Prototypes;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusSolutionAnalyzerSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly VirusDiagnoserConsoleSystem _console = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly VirusDiagnoserDataServerSystem _dataServer = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
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
        if (args.Port == ent.Comp.VirusDiagnoserPort)
            ent.Comp.ConnectedConsole = null;
    }

    private void OnAnchor(Entity<VirusSolutionAnalyzerComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (ent.Comp.ConnectedConsole == null || !TryComp<VirusDiagnoserConsoleComponent>(ent.Comp.ConnectedConsole, out var console))
            return;

        if (args.Anchored)
        {
            _console.RecheckConnections((ent.Comp.ConnectedConsole.Value, console));
            return;
        }

        _console.UpdateUserInterface((ent.Comp.ConnectedConsole.Value, console));
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

    public void StartPrinting(Entity<VirusSolutionAnalyzerComponent?> ent, VirusData? data)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.VirusDataCPU = data;
        SetStatus((ent, ent.Comp), VirusSolutionAnalyzerStatus.Printing);
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

        if (!_container.TryGetContainer(ent, FlaskContainerKey, out var flaskContainer))
            return;

        if (flaskContainer is not ContainerSlot slot)
            return;

        if (slot.ContainedEntity == null)
            return;


        if (!TryComp<SolutionContainerManagerComponent>(slot.ContainedEntity, out var solutionContainerManager))
            continue;

        if (!TryComp<DrawableSolutionComponent>(slot.ContainedEntity, out var injectable))
            continue;

        var entWrapper = new Entity<DrawableSolutionComponent?, SolutionContainerManagerComponent?>(slot.ContainedEntity, injectable, solutionContainerManager);

        if (!_solutionContainer.TryGetDrawableSolution(entWrapper, out Entity<SolutionComponent>? solutionEntity, out Solution? solution))
            continue;

        if (solutionEntity != null && solution != null)
        {
            _solutionContainer.TryAddReagent(solutionEntity.Value, Reagent, solution.MaxVolume, out _);
            
            var contents = solution.Contents;

            foreach (var reagent in contents)
            {
                var dataList = reagent.Reagent.Data;
                if (dataList == null)
                    continue;

                data = dataList.OfType<VirusData>().FirstOrDefault();
            }
        
            foreach (var reagent in solution.Contents)
            {
                if (reagent.Reagent.Prototype != Reagent)
                    continue;
                
                var dataList = reagent.Reagent.Data;
                if (dataList == null)
                    continue;

                data = dataList.OfType<VirusData>();

                foreach (var virusData in data)
                {
                    _dataServer.SaveData((console.VirusDiagnoserDataServer.Value, server), virusData);
                }
            }
        }

        _console.UpdateUserInterface((ent.Comp.ConnectedConsole.Value, console));
    }


    private void UpdateAppearance(Entity<VirusSolutionAnalyzerComponent> ent)
    {
        if (TryComp<AppearanceComponent>(ent, out var appearance))
            _appearance.SetData(ent, VirusSolutionAnalyzerVisuals.Status, ent.Comp.Status, appearance);
    }

    private void UpdateContainerAppearance(Entity<VirusSolutionAnalyzerComponent> ent)
    {
        if (_container.TryGetContainer(ent, FlaskContainerKey, out _))
            _appearance.SetData(ent, VirusSolutionContainerAnalyzerVisuals.Status, VirusSolutionContainerAnalyzerStatus.Fill, appearance);
        else
            _appearance.SetData(ent, VirusSolutionContainerAnalyzerVisuals.Status, VirusSolutionContainerAnalyzerStatus.Empty, appearance);
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
            case VirusSolutionAnalyzerStatus.Empty:

                break;
            case VirusSolutionAnalyzerStatus.Fill:

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