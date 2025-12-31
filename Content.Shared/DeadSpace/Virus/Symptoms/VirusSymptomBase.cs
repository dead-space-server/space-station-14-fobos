// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;

namespace Content.Shared.DeadSpace.Virus.Symptoms;

public abstract class VirusSymptomBase : IVirusSymptom
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    public TimedWindow EffectTimedWindow { get; }

    /// <summary>
    ///     Количество заразности, которое добавляет этот симптом.
    /// </summary>
    protected virtual float AddInfectivity { get; } = 0f;

    protected VirusSymptomBase(TimedWindow effectTimedWindow)
    {
        IoCManager.InjectDependencies(this);
        EffectTimedWindow = effectTimedWindow;
    }

    public abstract VirusSymptom Type { get; }

    public virtual void OnAdded(EntityUid host, VirusComponent virus)
    {
        var timedWindowSystem = _entityManager.System<TimedWindowSystem>();

        timedWindowSystem.Reset(EffectTimedWindow);
        virus.Data.Infectivity = Math.Clamp(virus.Data.Infectivity + AddInfectivity, 0, 1);
    }

    public virtual void OnRemoved(EntityUid host, VirusComponent virus)
    {
        var timedWindowSystem = _entityManager.System<TimedWindowSystem>();

        timedWindowSystem.Reset(EffectTimedWindow);
        virus.Data.Infectivity = Math.Clamp(virus.Data.Infectivity - AddInfectivity, 0, 1);
    }

    public virtual void OnUpdate(EntityUid host, VirusComponent virus)
    {
        var timedWindowSystem = _entityManager.System<TimedWindowSystem>();

        if (timedWindowSystem.IsExpired(EffectTimedWindow))
        {
            DoEffect(host, virus);

            if (!BaseVirusSettings.DebuffVirusMultipliers.TryGetValue(virus.RegenerationType, out var timeMultiplier) || timeMultiplier <= 0f)
                timeMultiplier = 1.0f;

            timedWindowSystem.Reset(
                EffectTimedWindow,
                EffectTimedWindow.MinSeconds * (1 / timeMultiplier),
                EffectTimedWindow.MaxSeconds * (1 / timeMultiplier)
            );
        }
    }

    public abstract void DoEffect(EntityUid host, VirusComponent virus);
    public abstract IVirusSymptom Clone();
    public virtual void ApplyDataEffect(VirusData data, bool add) { }
}
