// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Virus.Symptoms;

public sealed class LowChemicalAdaptationSymptom : VirusSymptomBase
{
    public override VirusSymptom Type => VirusSymptom.LowChemicalAdaptation;
    protected override float AddInfectivity => 0.05f;
    private float _addDefaultMedicineResistance = 0.15f;

    public LowChemicalAdaptationSymptom(IEntityManager entityManager, IGameTiming timing, IRobustRandom random, TimedWindow effectTimedWindow) : base(entityManager, timing, random, effectTimedWindow)
    { }

    public override void OnAdded(EntityUid host, VirusComponent virus)
    {
        base.OnAdded(host, virus);

        virus.Data.DefaultMedicineResistance += _addDefaultMedicineResistance;
    }

    public override void OnRemoved(EntityUid host, VirusComponent virus)
    {
        base.OnRemoved(host, virus);

        virus.Data.DefaultMedicineResistance -= _addDefaultMedicineResistance;
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
        return new LowChemicalAdaptationSymptom(EntityManager, Timing, Random, CloneTimedWindow());
    }
}
