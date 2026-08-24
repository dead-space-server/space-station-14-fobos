using Content.Shared.Botany.Components;
using Content.Shared.Botany.Events;
using Content.Shared.Botany.Items.Components;
using Content.Shared.Botany.Systems;
using Content.Shared.Botany.Traits.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Botany.Items.Systems;

/// <summary>
/// System for taking a sample of a plant.
/// </summary>
public sealed partial class BotanySampleTakerSystem : EntitySystem
{
    // DS14-start: current engine uses explicit event subscriptions.
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BotanySampleTakerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PlantComponent, PlantSampleAttemptEvent>(OnPlantSampleAttempt);
    }
    // DS14-end

    // DS14-start: current engine uses readonly IoC fields.
    [Dependency] private readonly BotanySystem _botany = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PlantHolderSystem _plantHolder = default!;
    [Dependency] private readonly PlantSystem _plant = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    // DS14-end

    // DS14-start: current engine uses readonly IoC fields.
    [Dependency] private readonly EntityQuery<PlantHolderComponent> _holderQuery = default!;
    [Dependency] private readonly EntityQuery<PlantDataComponent> _dataQuery = default!;
    [Dependency] private readonly EntityQuery<PlantHarvestComponent> _harvestQuery = default!;
    // DS14-end

    private void OnAfterInteract(Entity<BotanySampleTakerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || args.Handled || !args.CanReach || !HasComp<PlantComponent>(args.Target))
            return;

        var ev = new PlantSampleAttemptEvent(ent, args.User);
        RaiseLocalEvent(args.Target.Value, ref ev);

        args.Handled = true;
    }

    private void OnPlantSampleAttempt(Entity<PlantComponent> ent, ref PlantSampleAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!_holderQuery.TryComp(ent.Owner, out var holder)
            || !_dataQuery.TryComp(ent.Owner, out var plantData)
            || !_harvestQuery.TryComp(ent.Owner, out var harvest))
            return;

        if (_plantHolder.IsDead((ent.Owner, holder)))
        {
            _popup.PopupCursor(Loc.GetString("plant-sample-component-dead-plant-popup"), args.User);
            return;
        }

        // Prevent early sampling.
        var growthStage = _plant.GetGrowthStageValue(ent.AsNullable());
        if (growthStage <= args.Sample.Comp.MinSampleStage)
        {
            _popup.PopupCursor(Loc.GetString("plant-sample-component-early-sample-popup"), args.User);
            return;
        }

        var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));

        // Damage the plant.
        _plantHolder.AdjustsHealth((ent.Owner, holder), -random.NextFloat(args.Sample.Comp.SampleDamage.Min, args.Sample.Comp.SampleDamage.Max));

        // Produce a seed packet snapshot.
        float? healthOverride = harvest.ReadyForHarvest ? null : holder.Health;
        var protoId = MetaData(ent.Owner).EntityPrototype!.ID;
        _botany.SpawnSeedPacket(plantData, protoId, ent.Owner, Transform(args.User).Coordinates, args.User, healthOverride);

        var name = Loc.GetString(plantData.Name);
        _popup.PopupCursor(Loc.GetString("plant-sample-component-take-sample-popup", ("seedName", name)), args.User);

        if (random.Prob(args.Sample.Comp.SampleProbability))
            EnsureComp<PlantTraitSampledComponent>(ent.Owner);
    }
}
