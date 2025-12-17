// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Timing;
using Robust.Shared.Random;

namespace Content.Shared.DeadSpace.TimeWindow;

public sealed class TimedWindow
{
    public readonly IRobustRandom Random;
    public readonly IGameTiming Timing;
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
        Timing = timing;
        Random = random;
        Reset();
    }

    /// <summary>
    ///     Добавить время окну.
    /// </summary>
    public void AddTime(TimeSpan time)
    {
        Remaining += time;
    }

    /// <summary>
    ///     Сбрасывает таймер на новое случайное время.
    /// </summary>
    public void Reset()
    {
        Remaining = Timing.CurTime + GetRandomDuration();
    }

    /// <summary>
    ///     Сбрасывает таймер на заданный диапазон времени.
    /// </summary>
    public void Reset(float minSeconds, float maxSeconds)
    {
        Remaining = Timing.CurTime + GetRandomDuration(minSeconds, maxSeconds);
    }

    /// <summary>
    ///     Проверяет, истекло ли время окна.
    /// </summary>
    public bool IsExpired()
    {
        return Timing.CurTime >= Remaining;
    }

    /// <summary>
    ///     Проверяет, что окно либо null, либо истекло.
    /// </summary>
    public static bool NullOrExpired(TimedWindow? window)
    {
        return window == null || window.IsExpired();
    }

    private TimeSpan GetRandomDuration()
    {
        if (MinSeconds == MaxSeconds)
            return TimeSpan.FromSeconds(MinSeconds);

        var seconds = Random.NextFloat(MinSeconds, MaxSeconds);
        return TimeSpan.FromSeconds(seconds);
    }

    private TimeSpan GetRandomDuration(float minSeconds, float maxSeconds)
    {
        if (minSeconds == maxSeconds)
            return TimeSpan.FromSeconds(MinSeconds);

        var seconds = Random.NextFloat(minSeconds, MaxSeconds);
        return TimeSpan.FromSeconds(seconds);
    }
}
