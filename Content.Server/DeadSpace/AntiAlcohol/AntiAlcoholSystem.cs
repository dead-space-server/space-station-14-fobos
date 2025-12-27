using System;
using System.Collections.Generic;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Medical;
using Content.Shared.DeadSpace.AntiAlcohol;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;

namespace Content.Server.DeadSpace.AntiAlcohol;

public sealed class AntiAlcoholSystem : EntitySystem
{
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    private readonly List<(EntityUid Target, TimeSpan ExecuteAt)> _pendingVomit = new();
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExecuteEntityEffectEvent<AntiAlcoholImplantEffect>>(OnExecuteAntiAlcoholEffect);
    }

    private void OnExecuteAntiAlcoholEffect(ref ExecuteEntityEffectEvent<AntiAlcoholImplantEffect> args)
    {
        if (args.Args is not EntityEffectReagentArgs reagentArgs)
            return;

        if (!TryComp(reagentArgs.TargetEntity, out AntiAlcoholWatcherComponent? watcher))
            return;

        if (reagentArgs.Reagent is not { } reagent || reagentArgs.Source is not { } solution)
            return;

        var reagentId = reagent.ID;
        var ethanolAmount = solution.GetTotalPrototypeQuantity(reagentId);
        var threshold = FixedPoint2.New(watcher.Threshold);

        if (ethanolAmount < threshold)
            return;

        if (!_random.Prob(watcher.Probability))
            return;

        var target = reagentArgs.TargetEntity;
        _vomit.Vomit(target);
        var poisonDamage = new DamageSpecifier();
        poisonDamage.DamageDict.Add("Poison", 5);
        _damageableSystem.TryChangeDamage(target, poisonDamage);
    }
}
