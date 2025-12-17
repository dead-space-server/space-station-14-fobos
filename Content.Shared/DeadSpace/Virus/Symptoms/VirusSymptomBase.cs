// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Virus;

namespace Content.Shared.DeadSpace.Virus.Symptoms;

public abstract class VirusSymptomBase : IVirusSymptom
{
    protected readonly IEntityManager EntityManager;
    protected readonly IGameTiming Timing;
    protected readonly IRobustRandom Random;
    public TimedWindow EffectTimedWindow { get; }

    /// <summary>
    ///     Количество заразности, которое добавляет этот симптом.
    /// </summary>
    protected virtual float AddInfectivity { get; } = 0f;

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
        virus.Data.Infectivity = Math.Clamp(virus.Data.Infectivity + AddInfectivity, 0, 1);
    }

    public virtual void OnRemoved(EntityUid host, VirusComponent virus)
    {
        virus.Data.Infectivity = Math.Clamp(virus.Data.Infectivity - AddInfectivity, 0, 1);
    }

    public virtual void OnUpdate(EntityUid host, VirusComponent virus)
    {
        if (EffectTimedWindow.IsExpired())
        {
            DoEffect(host, virus);

            if (!BaseVirusSettings.DebuffVirusMultipliers.TryGetValue(virus.RegenerationType, out var timeMultiplier))
                timeMultiplier = 1.0f;

            EffectTimedWindow.Reset(
                EffectTimedWindow.MinSeconds * 1 / timeMultiplier,
                EffectTimedWindow.MaxSeconds * 1 / timeMultiplier
            );

            EffectTimedWindow.Reset();
        }
    }

    public abstract void DoEffect(EntityUid host, VirusComponent virus);
    public abstract IVirusSymptom Clone();

    protected TimedWindow CloneTimedWindow()
    {
        return new TimedWindow(
            EffectTimedWindow.MinSeconds,
            EffectTimedWindow.MaxSeconds,
            EffectTimedWindow.Timing,
            EffectTimedWindow.Random
        );
    }
}
