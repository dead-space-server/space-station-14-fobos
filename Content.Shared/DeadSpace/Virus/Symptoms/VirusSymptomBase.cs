// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Virus;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Virus.Symptoms;

public abstract class VirusSymptomBase : IVirusSymptom
{
    protected readonly IEntityManager EntityManager;
    protected readonly IGameTiming Timing;
    protected readonly IRobustRandom Random;
    public TimedWindow EffectTimedWindow { get; }
    protected abstract ProtoId<VirusSymptomPrototype> PrototypeId { get; }

    protected VirusSymptomBase(IEntityManager entityManager, IGameTiming timing, IRobustRandom random, TimedWindow effectTimedWindow)
    {
        EntityManager = entityManager;
        Timing = timing;
        EffectTimedWindow = effectTimedWindow;
        Random = random;
    }

    public abstract VirusSymptom Type { get; }

    public virtual void OnAdded(EntityUid host, VirusComponent virus)
    {
        ApplyDataEffect(virus.Data, true);
    }

    public virtual void OnRemoved(EntityUid host, VirusComponent virus)
    {
        ApplyDataEffect(virus.Data, false);
    }

    public virtual void OnUpdate(EntityUid host, VirusComponent virus)
    {
        if (EffectTimedWindow.IsExpired())
        {
            DoEffect(host, virus);

            if (!BaseVirusSettings.DebuffVirusMultipliers.TryGetValue(virus.RegenerationType, out var timeMultiplier) || timeMultiplier <= 0f)
                timeMultiplier = 1.0f;

            EffectTimedWindow.Reset(
                EffectTimedWindow.MinSeconds * (1 / timeMultiplier),
                EffectTimedWindow.MaxSeconds * (1 / timeMultiplier)
            );
        }
    }

    public abstract void DoEffect(EntityUid host, VirusComponent virus);
    public abstract IVirusSymptom Clone();
    public virtual void ApplyDataEffect(VirusData data, bool add)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        if (!prototypeManager.TryIndex(PrototypeId, out var prototype))
            return;

        if (add)
            data.Infectivity = Math.Clamp(data.Infectivity + prototype.AddInfectivity, 0, 1);
        else
            data.Infectivity = Math.Clamp(data.Infectivity - prototype.AddInfectivity, 0, 1);
    }
}
