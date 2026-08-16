using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Server.DeadSpace.Ninja.Systems;

public sealed class InjectReagentsActionSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InjectReagentsActionComponent, UseInjectReagentsActionEvent>(OnUse);
    }

    private void OnUse(Entity<InjectReagentsActionComponent> ent, ref UseInjectReagentsActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_solution.TryGetInjectableSolution(args.Performer, out var solution, out _))
            return;

        var injected = false;
        foreach (var (reagent, amount) in ent.Comp.Reagents)
        {
            injected |= _solution.TryAddReagent(solution.Value, reagent, amount, out _);
        }

        if (!injected)
            return;

        if (ent.Comp.Popup is { } popup)
            _popup.PopupEntity(Loc.GetString(popup), args.Performer, args.Performer, PopupType.Medium);

        _audio.PlayEntity(ent.Comp.InjectSound, args.Performer, args.Performer);

        args.Handled = true;
    }
}