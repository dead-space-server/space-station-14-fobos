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
using Content.Shared.DeadSpace.Virus;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Content.Shared.Body.Prototypes;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusDiagnoserSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly VirusDiagnoserConsoleSystem _console = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly VirusDiagnoserDataServerSystem _dataServer = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly TimedWindowSystem _timedWindowSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    private const string DnaContainerKey = "dna_container_virus_diagnoser";
    private const string FlaskContainerKey = "flask_container_virus_diagnoser";
    public ProtoId<ReagentPrototype> Reagent = "ViralSolution";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusDiagnoserComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<VirusDiagnoserComponent, AnchorStateChangedEvent>(OnAnchor);
        SubscribeLocalEvent<VirusDiagnoserComponent, PortDisconnectedEvent>(OnPortDisconnected);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VirusDiagnoserComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_powerReceiverSystem.IsPowered(uid))
            {
                SetStatus((uid, comp), VirusDiagnoserStatus.Off);
                continue; // без питания ничего не делаем
            }

            // Если был выключен — включаем
            if (comp.Status == VirusDiagnoserStatus.Off)
                SetStatus((uid, comp), VirusDiagnoserStatus.On);

            if (EntityManager.EntityExists(comp.CurrentSoundEntity) && comp.Status != VirusDiagnoserStatus.Printing)
                continue;

            switch (comp.Status)
            {
                case VirusDiagnoserStatus.Printing:
                    if (_timedWindowSystem.IsExpired(comp.AnimationWindow))
                    {
                        EndPrintingReport((uid, comp));
                        SetStatus((uid, comp), VirusDiagnoserStatus.On);
                    }
                    break;

                case VirusDiagnoserStatus.Scanning:
                    if (!CanScanning((uid, comp)))
                    {
                        SetStatus((uid, comp), VirusDiagnoserStatus.Denial);
                        break;
                    }

                    EndScanVirus((uid, comp));
                    break;

                case VirusDiagnoserStatus.GenerateVirus:
                    if (!CanGenerateVirus((uid, comp)))
                    {
                        SetStatus((uid, comp), VirusDiagnoserStatus.Denial);
                        break;
                    }

                    EndGenerateVirus((uid, comp));
                    break;

                case VirusDiagnoserStatus.Denial:
                    SetStatus((uid, comp), VirusDiagnoserStatus.On);
                    break;

                case VirusDiagnoserStatus.Successfully:
                    SetStatus((uid, comp), VirusDiagnoserStatus.On);
                    break;

                case VirusDiagnoserStatus.On:
                default:
                    break;
            }

        }
    }

    private void OnPortDisconnected(Entity<VirusDiagnoserComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port == ent.Comp.VirusDiagnoserPort)
            ent.Comp.ConnectedConsole = null;
    }

    private void OnAnchor(Entity<VirusDiagnoserComponent> ent, ref AnchorStateChangedEvent args)
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

    private void OnExamine(EntityUid uid, VirusDiagnoserComponent component, ExaminedEvent args)
    {
        BaseContainer? container = default!;

        if (_container.TryGetContainer(uid, DnaContainerKey, out container))
        {

            if (container is ContainerSlot slot)
            {
                if (slot.ContainedEntity != null)
                    args.PushMarkup(Loc.GetString("virus-diagnoser-dna-material-attached"));
            }
        }

        if (_container.TryGetContainer(uid, FlaskContainerKey, out container))
        {
            if (container is ContainerSlot slot)
            {
                if (slot.ContainedEntity != null)
                    args.PushMarkup(Loc.GetString("virus-diagnoser-flask-attached"));
            }
        }
    }

    public void StartPrinting(Entity<VirusDiagnoserComponent?> ent, VirusData? data)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.VirusDataCPU = data;
        SetStatus((ent, ent.Comp), VirusDiagnoserStatus.Printing);
    }

    public void StartScanVirus(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (!CanScanning((ent, ent.Comp)))
        {
            SetStatus((ent, ent.Comp), VirusDiagnoserStatus.Denial);
            return;
        }

        SetStatus((ent, ent.Comp), VirusDiagnoserStatus.Scanning);
    }

    private void EndPrintingReport(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        var data = ent.Comp.VirusDataCPU;

        var paper = Spawn(ent.Comp.Paper, Transform(ent).Coordinates);
        if (!TryComp<PaperComponent>(paper, out var paperComp))
        {
            QueueDel(paper);
            return;
        }

        if (data == null)
        {
            var noVirusText = Loc.GetString("virus-report-no-virus");

            _paperSystem.SetContent((paper, paperComp), noVirusText);
            return;
        }

        // Собираем текст отчёта

        // 1) симптомы
        var symptomsText =
            data.ActiveSymptom.Count == 0
                ? Loc.GetString("virus-report-symptoms-none")
                : string.Join(", ", data.ActiveSymptom.Select(symptom =>
                {
                    // Получаем строковый ID,
                    var id = symptom.ToString();

                    // Если нашли прототип — возвращаем Name
                    if (_prototypeManager.TryIndex<VirusSymptomPrototype>(id, out var proto))
                        return proto.Name;

                    // Если прототипа нет — fallback на ToString()
                    return id;
                }));

        // 2) виды (BodyWhitelist)
        string bodyText;
        if (data.BodyWhitelist == null || data.BodyWhitelist.Count == 0)
        {
            bodyText = Loc.GetString("virus-report-body-any");
        }
        else
        {
            var names = new List<string>();
            foreach (var protoId in data.BodyWhitelist)
            {
                if (_prototypeManager.TryIndex(protoId, out BodyPrototype? sp))
                {
                    // используем локализованное имя, если доступно; иначе ID
                    var display = sp?.Name ?? protoId.ToString();
                    names.Add(display);
                }
                else
                {
                    names.Add(protoId.ToString());
                }
            }

            bodyText = string.Join(", ", names);
        }

        // 3) медицина
        string medicineText;
        if (data.MedicineResistance == null || data.MedicineResistance.Count == 0)
        {
            medicineText = Loc.GetString("virus-report-medicine-none");
        }
        else
        {
            var lines = new List<string>();
            foreach (var kvp in data.MedicineResistance)
            {
                var reagentId = kvp.Key;
                var value = kvp.Value;

                if (_prototypeManager.TryIndex(reagentId, out var rp))
                {
                    var reagentName = rp.LocalizedName;
                    lines.Add(Loc.GetString("virus-report-medicine-entry", ("name", reagentName), ("value", value.ToString("0.00"))));
                }
                else
                {
                    lines.Add(Loc.GetString("virus-report-medicine-entry", ("name", reagentId.ToString()), ("value", value.ToString("0.00"))));
                }
            }

            medicineText = string.Join("\n", lines);
        }

        var content = $@"
        [center][b]{Loc.GetString("virus-report-title")}[/b][/center]

        {Loc.GetString("virus-report-strain", ("id", data.StrainId))}

        {Loc.GetString("virus-report-threshold", ("value", data.MaxThreshold.ToString("0.0")))}
        {Loc.GetString("virus-report-infectivity", ("value", (data.Infectivity * 100).ToString("0")))}

        {Loc.GetString("virus-report-damage-when-dead", ("value", data.DamageWhenDead.ToString("0.0")))}
        {Loc.GetString("virus-report-mutation-points", ("value", (data.MutationPoints).ToString("0")))}
        {Loc.GetString("virus-report-regen-threshold", ("value", data.RegenThreshold.ToString("0.0")))}
        {Loc.GetString("virus-report-regen-mutation", ("value", data.RegenMutationPoints.ToString("0.0")))}
        {Loc.GetString("virus-report-milty-price-delete-symptom", ("value", data.MultiPriceDeleteSymptom.ToString("0.0")))}

        {Loc.GetString("virus-report-default-medicine-resistance", ("value", data.DefaultMedicineResistance.ToString("0.00")))}

        {Loc.GetString("virus-report-medicine-header")}
        {medicineText}

        {Loc.GetString("virus-report-symptoms-header")}
        {(string.IsNullOrWhiteSpace(symptomsText) ? Loc.GetString("virus-report-symptoms-none") : symptomsText)}

        {Loc.GetString("virus-report-bodyes-header")}
        {bodyText}

        [small]{Loc.GetString("virus-report-footer")}[/small]
        ";

        _paperSystem.SetContent((paper, paperComp), content);
    }

    private void EndScanVirus(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        SetStatus((ent, ent.Comp), VirusDiagnoserStatus.Successfully);

        if (!_container.TryGetContainer(ent, DnaContainerKey, out var dnaContainer))
            return;

        if (dnaContainer is not ContainerSlot slot)
            return;

        if (slot.ContainedEntity == null)
            return;

        if (!TryComp<VirusDataCollectorComponent>(slot.ContainedEntity, out var dataCol))
            return;

        if (dataCol.Data == null)
            return;

        if (ent.Comp.ConnectedConsole == null || !TryComp<VirusDiagnoserConsoleComponent>(ent.Comp.ConnectedConsole, out var console))
            return;

        if (!TryComp<VirusDiagnoserDataServerComponent>(console.VirusDiagnoserDataServer, out var server))
            return;

        _dataServer.SaveData((console.VirusDiagnoserDataServer.Value, server), dataCol.Data);

        _container.CleanContainer(dnaContainer);

        _console.UpdateUserInterface((ent.Comp.ConnectedConsole.Value, console));
    }

    private void EndGenerateVirus(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        SetStatus((ent, ent.Comp), VirusDiagnoserStatus.Successfully);

        if (ent.Comp.VirusDataCPU == null)
            return;

        if (!_container.TryGetContainer(ent, FlaskContainerKey, out var dnaContainer))
            return;

        if (dnaContainer is not ContainerSlot slot)
            return;

        if (slot.ContainedEntity == null)
            return;

        var ents = _container.EmptyContainer(dnaContainer);

        foreach (var flask in ents)
        {
            if (!TryComp<SolutionContainerManagerComponent>(flask, out var solutionContainerManager))
                continue;

            if (!TryComp<DrawableSolutionComponent>(flask, out var injectable))
                continue;

            var entWrapper = new Entity<DrawableSolutionComponent?, SolutionContainerManagerComponent?>(flask, injectable, solutionContainerManager);

            if (!_solutionContainer.TryGetDrawableSolution(entWrapper, out Entity<SolutionComponent>? solutionEntity, out Solution? solution))
                continue;

            if (solutionEntity != null && solution != null)
            {
                _solutionContainer.TryAddReagent(solutionEntity.Value, Reagent, solution.MaxVolume, out _);

                foreach (var reagent in solution.Contents)
                {
                    if (reagent.Reagent.Prototype != Reagent)
                        continue;

                    List<ReagentData> reagentData = reagent.Reagent.EnsureReagentData();

                    reagentData.RemoveAll(x => x is VirusData);

                    reagentData.Add(ent.Comp.VirusDataCPU);
                }
            }
        }

    }

    public void StartGenerateVirus(Entity<VirusDiagnoserComponent?> ent, VirusData? data = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (!CanGenerateVirus((ent, ent.Comp)) || data == null)
        {
            SetStatus((ent, ent.Comp), VirusDiagnoserStatus.Denial);
            return;
        }

        ent.Comp.VirusDataCPU = data;
        SetStatus((ent, ent.Comp), VirusDiagnoserStatus.GenerateVirus);
    }

    private void UpdateAppearance(Entity<VirusDiagnoserComponent> ent)
    {
        if (TryComp<AppearanceComponent>(ent, out var appearance))
            _appearance.SetData(ent, VirusDiagnoserVisuals.Status, ent.Comp.Status, appearance);
    }

    private void SetStatus(Entity<VirusDiagnoserComponent?> ent, VirusDiagnoserStatus newStatus)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (ent.Comp.Status == newStatus)
            return;

        if (newStatus != VirusDiagnoserStatus.On)
            QueueDel(ent.Comp.CurrentSoundEntity);

        ent.Comp.CurrentSoundEntity = null;

        switch (newStatus)
        {
            case VirusDiagnoserStatus.On:

                break;
            case VirusDiagnoserStatus.Off:
                break;
            case VirusDiagnoserStatus.Printing:
                _timedWindowSystem.Reset(ent.Comp.AnimationWindow);
                ent.Comp.CurrentSoundEntity = _audio.PlayPvs(ent.Comp.PrintingSound, ent)?.Entity;
                break;
            case VirusDiagnoserStatus.Scanning:
                ent.Comp.CurrentSoundEntity = _audio.PlayPvs(ent.Comp.ScanningSound, ent)?.Entity;
                break;
            case VirusDiagnoserStatus.Denial:
                ent.Comp.CurrentSoundEntity = _audio.PlayPvs(ent.Comp.DenialSound, ent)?.Entity;
                break;
            case VirusDiagnoserStatus.Successfully:
                ent.Comp.CurrentSoundEntity = _audio.PlayPvs(ent.Comp.SuccessfullySound, ent)?.Entity;
                break;
            case VirusDiagnoserStatus.GenerateVirus:
                ent.Comp.CurrentSoundEntity = _audio.PlayPvs(ent.Comp.GenerateVirusSound, ent)?.Entity;
                break;
            default:

                break;
        }

        ent.Comp.Status = newStatus;

        UpdateAppearance((ent, ent.Comp));
    }

    public bool CanScanning(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!_container.TryGetContainer(ent, DnaContainerKey, out var dnaContainer))
            return false;

        if (dnaContainer is not ContainerSlot slot)
            return false;

        if (slot.ContainedEntity == null)
            return false;

        return true;
    }

    public bool CanGenerateVirus(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!_container.TryGetContainer(ent, FlaskContainerKey, out var container))
            return false;

        if (container is not ContainerSlot slot)
            return false;

        if (slot.ContainedEntity == null)
            return false;

        return true;
    }

}