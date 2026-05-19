using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Implants.Components;
using Content.Shared.Popups;
// its just a copie of adrenalimplant..
namespace Content.Server.DeadSpace.Implants.Rezerv;

public sealed class RezervImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SubdermalImplantComponent, UseRezervImplantEvent>(OnRezervActivated);
    }

    private void OnRezervActivated(EntityUid uid, SubdermalImplantComponent component, UseRezervImplantEvent args)
    {
        if (component.ImplantedEntity is not { } target)
            return;

        var reagents = new List<(string, FixedPoint2)>()
        {
            ("Desoxyephedrine", 15),
            ("Dylovene", 15)
        };

        var solution = new Shared.Chemistry.Components.Solution();
        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Item1, reagent.Item2);

        if (_solution.TryGetInjectableSolution(target, out var injectable, out _))
        {
            _solution.TryAddSolution(injectable.Value, solution);
            _popup.PopupEntity(Loc.GetString("rezerv-implant-activated"), target, target);
        }

        args.Handled = true;
    }
}