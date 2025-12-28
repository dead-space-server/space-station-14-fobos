// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.Necromorphs.InfectionDead.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.Zombies;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Virus.Symptoms;

public sealed class ZombificationSymptom : VirusSymptomBase
{
    public override VirusSymptom Type => VirusSymptom.Zombification;
    protected override ProtoId<VirusSymptomPrototype> PrototypeId => "ZombificationSymptom";

    public ZombificationSymptom(IEntityManager entityManager, IGameTiming timing, IRobustRandom random, TimedWindow effectTimedWindow) : base(entityManager, timing, random, effectTimedWindow)
    { }

    public override void OnAdded(EntityUid host, VirusComponent virus)
    {
        base.OnAdded(host, virus);

        InfectZombieVirus(host);
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
        InfectZombieVirus(host);
    }

    private void InfectZombieVirus(EntityUid target)
    {
        if (EntityManager.HasComponent<ZombieComponent>(target) || EntityManager.HasComponent<ZombieImmuneComponent>(target))
            return;

        // DS14-start
        if (EntityManager.HasComponent<NecromorfComponent>(target) || EntityManager.HasComponent<InfectionDeadComponent>(target))
            return;

        EntityManager.EnsureComponent<PendingZombieComponent>(target);
        EntityManager.EnsureComponent<ZombifyOnDeathComponent>(target);
    }

    public override void ApplyDataEffect(VirusData data, bool add)
    {
        base.ApplyDataEffect(data, add);
    }

    public override IVirusSymptom Clone()
    {
        return new ZombificationSymptom(EntityManager, Timing, Random, EffectTimedWindow.Clone());
    }
}
