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

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusDiagnoserDataServerSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IngestionSystem _ingestion = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly VirusDiagnoserConsoleSystem _console = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusDiagnoserDataServerComponent, AnchorStateChangedEvent>(OnDoAfter);
        SubscribeLocalEvent<VirusDiagnoserDataServerComponent, PortDisconnectedEvent>(OnDoAfter);
    }

    private void OnPortDisconnected(Entity<VirusDiagnoserDataServerComponent> ent, ref PortDisconnectedEvent args)
    {   
        if (args.Port == ent.Comp.VirusDiagnoserDataServerReceiver)
            ent.Comp.ConnectedConsole = null;
    }

    private void OnAnchor(Entity<VirusDiagnoserDataServerComponent> ent, ref AnchorStateChangedEvent args)
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
}
