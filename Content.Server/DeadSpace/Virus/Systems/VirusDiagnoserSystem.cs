// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Content.Shared.Examine;
using Robust.Shared.Containers;
using Content.Server.DeadSpace.Virus.Components;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusDiagnoserSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly VirusDiagnoserConsoleSystem _console = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private const string DnaContainerKey = "dna-container-virus-diagnoser";
    private const string FlaskContainerKey = "flask-container-virus-diagnoser";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusDiagnoserComponent, ExaminedEvent>(OnAfterInteract);
        SubscribeLocalEvent<VirusDiagnoserComponent, AnchorStateChangedEvent>(OnDoAfter);
        SubscribeLocalEvent<VirusDiagnoserComponent, PortDisconnectedEvent>(OnDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VirusDiagnoserComponent>();
        while (query.MoveNext(out var uid, out var component))
        {

            if (!_powerReceiverSystem.IsPowered(uid))
            {
                SetStatus((uid, component), VirusDiagnoserStatus.Off);
            }
            else if (component.Status == VirusDiagnoserStatus.Off)
            {
                SetStatus((uid, component), VirusDiagnoserStatus.On);
            }

            if (component.Status = VirusDiagnoserStatus.Printing)
            {
                if (!_entityManager.EntityExists(component.PrintingSoundEntity))
                {
                    PrintReport((uid, component));
                    SetStatus((uid, component), VirusDiagnoserStatus.On);
                }
            }

            if (component.Status = VirusDiagnoserStatus.Scanning)
            {
                if (!CanScanning((ent, ent.Comp)))
                    SetStatus((uid, component), VirusDiagnoserStatus.Deniel);

                if (!_entityManager.EntityExists(component.ScanningSoundEntity))
                {

                    SetStatus((uid, component), VirusDiagnoserStatus.On);
                }
            }

            if (component.Status = VirusDiagnoserStatus.GenerateVirus)
            {
                if (!CanGenerateVirus((ent, ent.Comp)))
                    SetStatus((uid, component), VirusDiagnoserStatus.Deniel);

                if (!_entityManager.EntityExists(component.GenerateVirusSoundEntity))
                {
                    ScanVirus((uid, component));
                    SetStatus((uid, component), VirusDiagnoserStatus.On);
                }
            }

            if (component.Status = VirusDiagnoserStatus.Deniel)
            {
                if (!_entityManager.EntityExists(component.ScaningSoundEntity))
                    SetStatus((uid, component), VirusDiagnoserStatus.On);
            }

            if (component.Status = VirusDiagnoserStatus.Successfully)
            {
                if (!_entityManager.EntityExists(component.ScaningSoundEntity))
                    SetStatus((uid, component), VirusDiagnoserStatus.On);
            }
        }
    }

    private void OnPortDisconnected(Entity<VirusDiagnoserComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port == ent.Comp.VirusDiagnoserReceiver)
            ent.Comp.ConnectedConsole = null;
    }

    private void OnAnchor(Entity<VirusDiagnoserComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (ent.Comp.ConnectedConsole == null || !TryComp<VirusDiagnoserConsoleComponent>(ent.Comp.ConnectedConsole, out var console))
            return;

        if (args.Anchored)
        {
            _console.RecheckConnections(ent.Comp.ConnectedConsole.Value, ent.Owner, console.GeneticScanner, console);
            return;
        }
        _console.UpdateUserInterface((ent.Comp.ConnectedConsole.Value, console));
    }

    private void OnExamine(EntityUid uid, VirusDiagnoserComponent component, ExaminedEvent args)
    {
        if (_container.TryGetContainer(uid, DnaContainerKey, out var container))
        {

            if (container is ContainerSlot slot)
            {
                if (slot.ContainedEntity != null)
                    args.PushMarkup(Loc.GetString("virus-diagnoser-dna-material-attached"));
            }
        }

        if (_container.TryGetContainer(uid, FlaskContainerKey, out var container))
        {

            if (container is ContainerSlot slot)
            {
                if (slot.ContainedEntity != null)
                    args.PushMarkup(Loc.GetString("virus-diagnoser-flask-attached"));
            }
        }
    }

    public void SetAppearance(EntityUid uid, DrugInitializeMachineVisualState state, DrugInitializeMachineComponent? component = null, AppearanceComponent? appearanceComponent = null)
    {
        if (!Resolve(uid, ref component, ref appearanceComponent, false))
            return;

        _appearance.SetData(uid, PowerDeviceVisuals.VisualState, state, appearanceComponent);
    }

    public void OnStartInitialize(EntityUid uid, DrugInitializeMachineComponent component, StartDrugInitializeEvent args)
    {
        if (component.IsRunning)
            return;

        if (!TryComp<DrugTestStickComponent>(args.Stick, out var drugTestStick))
            return;

        _container.Insert(args.Stick, component.Tube);

        SetAppearance(uid, DrugInitializeMachineVisualState.Running, component);
        _audio.PlayPvs(component.PrintingSound, uid);
        component.IsRunning = true;
        component.RunningTime = _gameTiming.CurTime + component.DurationRunning;
    }

    private void StartScanVirus(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return default!;
        
        if (!CanScanning((ent, ent.Comp)))
        {
            SetStatus((uid, component), VirusDiagnoserStatus.Deniel);
            return;
        }
        
        SetStatus((uid, component), VirusDiagnoserStatus.Scanning);
    }

    private void EndPrintingReport(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        VirusData Data = dataCol.Data;

        var paper = Spawn(ent.Comp.Paper, Transform(uid).Coordinates);
        if (!TryComp<PaperComponent>(paper, out var paperComp))
        {
            QueueDel(paper);
            return;
        }

        // ----------------------
        // Собираем текст отчёта
        // ----------------------

        // 1) симптомы
        var symptomsText = Data.ActiveSymptomInstances.Count == 0
            ? Loc.GetString("virus-report-symptoms-none")
            : string.Join(", ", Data.ActiveSymptomInstances.Select(s => s.Type.ToString()));

        // 2) виды (SpeciesWhitelist)
        string speciesText;
        if (Data.SpeciesWhitelist == null || Data.SpeciesWhitelist.Count == 0)
        {
            speciesText = Loc.GetString("virus-report-species-any");
        }
        else
        {
            var names = new List<string>();
            foreach (var protoId in Data.SpeciesWhitelist)
            {
                if (_prototypeManager.TryIndex(protoId, out SpeciesPrototype? sp))
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

            speciesText = string.Join(", ", names);
        }

        // 3) медицина
        string medicineText;
        if (Data.MedicineResistance == null || Data.MedicineResistance.Count == 0)
        {
            medicineText = Loc.GetString("virus-report-medicine-none");
        }
        else
        {
            var lines = new List<string>();
            foreach (var kvp in Data.MedicineResistance)
            {
                var reagentId = kvp.Key;
                var value = kvp.Value;

                if (_prototypeManager.TryIndex(reagentId, out ReagentPrototype? rp))
                {
                    // ReagentPrototype
                    var reagentName = rp.LocalizedName ?? rp.Name;
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

        {Loc.GetString("virus-report-strain", ("id", Data.StrainId))}

        {Loc.GetString("virus-report-threshold", ("value", Data.Threshold.ToString("0.0")))}
        {Loc.GetString("virus-report-infectivity", ("value", (Data.Infectivity * 100).ToString("0")))}
        {Loc.GetString("virus-report-complexity", ("value", Data.ComplexityVaccine.ToString("0.0")))}

        {Loc.GetString("virus-report-default-medicine-resistance", ("value", Data.DefaultMedicineResistance.ToString("0.00")))}

        {Loc.GetString("virus-report-medicine-header")}
        {medicineText}

        {Loc.GetString("virus-report-symptoms-header")}
        {(string.IsNullOrWhiteSpace(symptomsText) ? Loc.GetString("virus-report-symptoms-none") : symptomsText)}

        {Loc.GetString("virus-report-species-header")}
        {speciesText}

        [small]{Loc.GetString("virus-report-footer")}[/small]
        ";

        // Применяем на бумагу
        _paperSystem.SetContent((paper, paperComp), content);
    }

    private void EndScanVirus(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        SetStatus((uid, component), VirusDiagnoserStatus.Successfully);

        if (!_container.TryGetContainer(target, DnaContainerKey, out var dnaContainer))
            return;

        if (container is not ContainerSlot slot)
            return;

        if (slot.ContainedEntity == null)
            return;

        _container.CleanContainer(dnaContainer);

        if (!TryComp<VirusDataCollectorComponent>(slot.ContainedEntity, out var dataCol))
            return;

        
    }

    private void EndGenerateVirus(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        
    }

    private void StartGenerateVirus(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (!CanGenerateVirus((ent, ent.Comp)))
        {
            SetStatus((uid, component), VirusDiagnoserStatus.Deniel);
            return;
        }
        
        SetStatus((uid, component), VirusDiagnoserStatus.GenerateVirus);
    }

    private void UpdateAppearance(Entity<VirusDiagnoserComponent> ent)
    {
        if (TryComp<AppearanceComponent>(uid, out var appearance))
            _appearance.SetData(uid, VirusDiagnoserVisuals.Status, ent.Comp.Status, appearance);
    }

    public void SetStatus(Entity<VirusDiagnoserComponent?> ent, VirusDiagnoserStatus newStatus)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        ent.Comp.Status = newStatus;

        switch (ent.Comp.Status)
        {
            case VirusDiagnoserStatus.On:

                break;
            case VirusDiagnoserStatus.Off:

                if (_entityManager.EntityExists(component.PrintingSoundEntity))
                    QueueDel(component.PrintingSoundEntity);

                if (_entityManager.EntityExists(component.ScanningSoundEntity))
                    QueueDel(component.ScanningSoundEntity);
                
                if (_entityManager.EntityExists(component.DenielSoundEntity))
                    QueueDel(component.DenielSoundEntity);

                if (_entityManager.EntityExists(component.SuccessfullySoundEntity))
                    QueueDel(component.SuccessfullySoundEntity);
                
                if (_entityManager.EntityExists(component.GenerateVirusSoundEntity))
                    QueueDel(component.GenerateVirusSoundEntity);

                break;
            case VirusDiagnoserStatus.Printing:
                ent.Comp.PrintingSoundEntity = _audio.PlayPvs(ent, ent.Comp.PrintingSound);
                break;
            case VirusDiagnoserStatus.Scanning:
                ent.Comp.ScanningSoundEntity = _audio.PlayPvs(ent, ent.Comp.ScanningSound);
                break;
            case VirusDiagnoserStatus.Deniel:
                ent.Comp.DenielSoundEntity = _audio.PlayPvs(ent, ent.Comp.DenielSound);
                break;
            case VirusDiagnoserStatus.Successfully:
                ent.Comp.SuccessfullySoundEntity = _audio.PlayPvs(ent, ent.Comp.SuccessfullySound);
                break;
            case VirusDiagnoserStatus.GenerateVirus:
                ent.Comp.GenerateVirusSoundEntity = _audio.PlayPvs(ent, ent.Comp.GenerateVirusSound);
                break;
            default:

                break;
        }

        UpdateAppearance((ent, ent.Comp));
    }

    public bool CanScanning(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;
        
        if (!_container.TryGetContainer(target, DnaContainerKey, out var dnaContainer))
            return false;
        
        if (dnaContainer is not ContainerSlot slot)
            return false;

        if (slot.ContainedEntity == null)
            return false;

        return true;
    }

    public bool CanGenerateVirus(Entity<VirusDiagnoserComponent?> ent)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;
        
        if (!_container.TryGetContainer(target, FlaskContainerKey, out var container))
            return false;
        
        if (container is not ContainerSlot slot)
            return false;

        if (slot.ContainedEntity == null)
            return false;

        return true;
    }

}