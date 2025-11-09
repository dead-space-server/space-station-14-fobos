// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Timing;
using Robust.Shared.Random;

namespace Content.Shared.DeadSpace.TimeWindow;

public sealed class TimedWindow
{
    private readonly IRobustRandom _random;
    private readonly IGameTiming _timing;
    public float MinSeconds { get; }
    public float MaxSeconds { get; }

    /// <summary>
    ///     Остаток времени до следующего события.
    /// </summary>
    public TimeSpan Remaining { get; private set; } = TimeSpan.Zero;

    public TimedWindow(float minSeconds, float maxSeconds, IGameTiming timing, IRobustRandom random)
    {
        MinSeconds = minSeconds;
        MaxSeconds = maxSeconds;
        _timing = timing;
        _random = random;
        Reset();
    }

    /// <summary>
    ///     Сбрасывает таймер на новое случайное время.
    /// </summary>
    public void Reset()
    {
        Remaining = _timing.CurTime + GetRandomDuration();
    }

    /// <summary>
    ///     Проверяет, истекло ли время окна.
    /// </summary>
    public bool IsExpired()
    {
        return _timing.CurTime >= Remaining;
    }

    private TimeSpan GetRandomDuration()
    {
        if (MinSeconds == MaxSeconds)
            return TimeSpan.FromSeconds(MinSeconds);

        var seconds = _random.NextFloat(MinSeconds, MaxSeconds);
        return TimeSpan.FromSeconds(seconds);
    }
}
