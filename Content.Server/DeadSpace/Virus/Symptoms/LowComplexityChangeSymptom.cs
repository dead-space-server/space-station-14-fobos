// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Server.DeadSpace.Virus.Systems;

namespace Content.Server.DeadSpace.Virus.Symptoms;

public sealed class LowComplexityChangeSymptom : VirusSymptomBase
{
    public override VirusSymptom Type => VirusSymptom.LowComplexityChange;
    protected override float AddInfectivity => 0.01f;
    private int _addMultiPriceDeleteSymptom = 1;

    public LowComplexityChangeSymptom(IEntityManager entityManager, IGameTiming timing, IRobustRandom random, TimedWindow effectTimedWindow) : base(entityManager, timing, random, effectTimedWindow)
    { }

    public override void OnAdded(EntityUid host, VirusComponent virus)
    {
        base.OnAdded(host, virus);

        var virusSystem = EntityManager.System<VirusSystem>();
        virusSystem.AddMultiPriceDeleteSymptom(virus.Data.StrainId, _addMultiPriceDeleteSymptom);
    }

    public override void OnRemoved(EntityUid host, VirusComponent virus)
    {
        base.OnRemoved(host, virus);

        var virusSystem = EntityManager.System<VirusSystem>();
        virusSystem.AddMultiPriceDeleteSymptom(virus.Data.StrainId, -_addMultiPriceDeleteSymptom);
    }

    public override void OnUpdate(EntityUid host, VirusComponent virus)
    {
        base.OnUpdate(host, virus);
    }

    public override void DoEffect(EntityUid host, VirusComponent virus)
    {

    }

    public override IVirusSymptom Clone()
    {
        return new LowComplexityChangeSymptom(EntityManager, Timing, Random, CloneTimedWindow());
    }
}
