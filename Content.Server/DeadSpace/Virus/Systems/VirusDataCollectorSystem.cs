// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.Virus;
using Content.Shared.Nutrition.EntitySystems;
using Content.Server.Popups;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Content.Shared.Humanoid.Prototypes;
using System.Linq;
using Content.Shared.Popups;
using Content.Shared.Forensics.Components;
using Content.Shared.Body.Prototypes;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusDataCollectorSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IngestionSystem _ingestion = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusDataCollectorComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<VirusDataCollectorComponent, CollectVirusDataDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<VirusDataCollectorComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (!CanBeUsed((entity, entity.Comp), target, args.User))
            return;

        _popup.PopupEntity(Loc.GetString("virus-collector-warn-target"), target, target, PopupType.Medium);

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(entity.Comp.Duration), new CollectVirusDataDoAfterEvent(), entity, target: target, used: entity)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = entity.Comp.Distance
        });

    }

    private void OnDoAfter(Entity<VirusDataCollectorComponent> entity, ref CollectVirusDataDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
            return;

        if (!TryComp<VirusComponent>(target, out var virus))
            return;

        if (TryComp<DnaComponent>(args.Target, out var dna))
            entity.Comp.DNA = dna.DNA ?? string.Empty;
        else
            entity.Comp.DNA = Loc.GetString("drug-collector-dna-not-found");

        // Собираем данные вируса
        var data = new VirusData
        {
            StrainId = virus.Data.StrainId,
            Threshold = virus.Data.Threshold,
            DefaultMedicineResistance = virus.Data.DefaultMedicineResistance,
            Infectivity = virus.Data.Infectivity,
            ActiveSymptom = virus.Data.ActiveSymptom,
            EntityWhitelist = virus.Data.EntityWhitelist,
            MedicineResistance = new Dictionary<ProtoId<ReagentPrototype>, float>(virus.Data.MedicineResistance),
            BodyWhitelist = virus.Data.BodyWhitelist
        };

        entity.Comp.Data = data;
        entity.Comp.IsUsed = true;

        args.Handled = true;
    }

    private bool CanBeUsed(Entity<VirusDataCollectorComponent?> source, EntityUid target, EntityUid user)
    {
        if (!Resolve(source, ref source.Comp, false))
            return false;

        if (source.Comp.IsUsed)
        {
            _popup.PopupEntity(Loc.GetString("virus-collector-is-used"), user, user);

            return false;
        }

        if (!_ingestion.HasMouthAvailable(user, target))
        {
            _popup.PopupEntity(Loc.GetString("virus-collector-no-mouth"), user, user);

            return false;
        }

        return true;
    }
}
