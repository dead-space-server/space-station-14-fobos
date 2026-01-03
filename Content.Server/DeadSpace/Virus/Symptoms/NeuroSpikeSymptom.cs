// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Jittering;
using Content.Server.Stunnable;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Virus.Symptoms;

public sealed class NeuroSpikeSymptom : VirusSymptomBase
{
    public override VirusSymptom Type => VirusSymptom.NeuroSpike;
    protected override ProtoId<VirusSymptomPrototype> PrototypeId => "NeuroSpikeSymptom";
    private TimedWindow _duration = default!;

    public NeuroSpikeSymptom(IEntityManager entityManager, IGameTiming timing, IRobustRandom random, TimedWindow effectTimedWindow) : base(entityManager, timing, random, effectTimedWindow)
    { }

    public override void OnAdded(EntityUid host, VirusComponent virus)
    {
        base.OnAdded(host, virus);

        _duration = new TimedWindow(5f, 10f, Timing, Random);
    }

    public override void OnRemoved(EntityUid host, VirusComponent virus)
    {
        base.OnRemoved(host, virus);
    }

    public override void OnUpdate(EntityUid host, VirusComponent virus)
    {
        base.OnUpdate(host, virus);
    }

    public override void DoEffect(EntityUid host, VirusComponent virus)
    {
        var jitteringSystem = EntityManager.System<SharedJitteringSystem>();
        var stun = EntityManager.System<StunSystem>();

        _duration.Reset();
        var duration = _duration.Remaining.TotalSeconds;

        jitteringSystem.DoJitter(host, TimeSpan.FromSeconds(duration), true);
        stun.TryUpdateParalyzeDuration(host, TimeSpan.FromSeconds(duration));
    }

    public override void ApplyDataEffect(VirusData data, bool add)
    {
        base.ApplyDataEffect(data, add);
    }

    public override IVirusSymptom Clone()
    {
        return new NeuroSpikeSymptom(EntityManager, Timing, Random, EffectTimedWindow.Clone());
    }
}
