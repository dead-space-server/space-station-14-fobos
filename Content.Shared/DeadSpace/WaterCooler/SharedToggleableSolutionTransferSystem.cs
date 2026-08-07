using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared.DeadSpace.WaterCooler;

public sealed class ToggleableSolutionTransferSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly Dictionary<EntityUid, TimeSpan> _lastPopup = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<ToggleableSolutionTransferComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ToggleableSolutionTransferComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<ToggleableSolutionTransferComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ToggleableSolutionTransferComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<ToggleableSolutionTransferComponent> ent, ref ComponentStartup args)
    {
        UpdateMode(ent);
    }

    private void OnMapInit(Entity<ToggleableSolutionTransferComponent> ent, ref MapInitEvent args)
    {
        UpdateMode(ent);
    }

    private void OnGetVerbs(Entity<ToggleableSolutionTransferComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var user = args.User;
        var isOutput = ent.Comp.Direction == SolutionTransferDirection.Output;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = isOutput ? "Перекчлючить в режим пополнения" : "Перекчлючить в режим раздачи",
            Act = () =>
            {
                // Защита от спама
                var now = _timing.CurTime;
                if (_lastPopup.TryGetValue(ent, out var last) && (now - last).TotalSeconds < 1.0)
                    return;

                // Меняем режим
                ent.Comp.Direction = isOutput
                    ? SolutionTransferDirection.Input
                    : SolutionTransferDirection.Output;
                UpdateMode(ent);
                Dirty(ent);

                _lastPopup[ent] = now;
            },
            Priority = 1,
        });
    }

    private void OnExamined(Entity<ToggleableSolutionTransferComponent> ent, ref ExaminedEvent args)
    {
        var directionText = ent.Comp.Direction switch
        {
            SolutionTransferDirection.Input => Loc.GetString("water-cooler-mode-intake"),
            SolutionTransferDirection.Output => Loc.GetString("water-cooler-mode-dispensing"),
            _ => string.Empty,
        };

        if (!string.IsNullOrEmpty(directionText))
            args.PushText(directionText);
    }

    private void UpdateMode(Entity<ToggleableSolutionTransferComponent> ent)
    {
        RemCompDeferred<DrainableSolutionComponent>(ent);
        RemCompDeferred<RefillableSolutionComponent>(ent);
        RemCompDeferred<SolutionTransferComponent>(ent);

        if (ent.Comp.Direction == SolutionTransferDirection.Input)
        {
            var refillable = EnsureComp<RefillableSolutionComponent>(ent);
            refillable.Solution = ent.Comp.Solution;
            Dirty(ent, refillable);
        }
        else
        {
            var drainable = EnsureComp<DrainableSolutionComponent>(ent);
            drainable.Solution = ent.Comp.Solution;
            Dirty(ent, drainable);
        }
    }
}
