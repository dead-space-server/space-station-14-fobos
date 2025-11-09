// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Virus.Symptoms;

public abstract class VirusSymptomBase : IVirusSymptom
{
    protected readonly IEntityManager EntityManager;
    protected readonly IGameTiming Timing;
    public TimedWindow EffectTimedWindow { get; }

    /// <summary>
    ///     Количество заразности, которое добавляет этот симптом.
    /// </summary>
    protected virtual float AddInfectivity { get; } = 0f;

    protected VirusSymptomBase(IEntityManager entityManager, IGameTiming timing, TimedWindow effectTimedWindow)
    {
        EntityManager = entityManager;
        Timing = timing;
        EffectTimedWindow = effectTimedWindow;
    }

    public abstract VirusSymptom Type { get; }

    public virtual void OnAdded(EntityUid host, VirusComponent virus)
    {
        virus.Infectivity = Math.Clamp(virus.Infectivity + AddInfectivity, 0, 1);
    }

    public virtual void OnRemoved(EntityUid host, VirusComponent virus)
    {
        virus.Infectivity = Math.Clamp(virus.Infectivity - AddInfectivity, 0, 1);
    }

    public virtual void OnUpdate(EntityUid host, VirusComponent virus)
    {
        if (EffectTimedWindow.IsExpired())
        {
            DoEffect(host, virus);
            EffectTimedWindow.Reset();
        }
    }

    public abstract void DoEffect(EntityUid host, VirusComponent virus);
    public abstract IVirusSymptom Clone();
}
