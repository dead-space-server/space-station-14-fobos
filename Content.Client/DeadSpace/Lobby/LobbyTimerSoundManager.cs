using Robust.Client.Audio;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Client.DeadSpace.Lobby;

/// <summary>
/// Manages the timer sound effects for the lobby, including ticking, pause/resume, and time modifications.
/// </summary>
public sealed class LobbyTimerSoundManager
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    private readonly AudioSystem _audioSys;

    /// <summary>
    /// Centralized resource paths for timer sound effects.
    /// </summary>
    private static class TimerSounds
    {
        private static readonly ResPath BasePath = ResPath.Root / "Audio" / "_DeadSpace" / "Lobby" / "Timer";

        public static readonly ResPath Tick = BasePath / "tick.ogg";
        public static readonly ResPath Pause = BasePath / "pause.ogg";
        public static readonly ResPath Resume = BasePath / "resume.ogg";
        public static readonly ResPath Increase = BasePath / "modified.ogg";
        public static readonly ResPath Decrease = BasePath / "modified.ogg";
    }

    /// <summary>
    /// Keeps track of the last second value when a tick sound was played.
    /// </summary>
    private int _lastTickSecond = -1;

    /// <summary>
    /// Stores the previous target (start) time to detect changes.
    /// </summary>
    private TimeSpan _lastStartTime;

    /// <summary>
    /// Remembers if the game was previously paused.
    /// </summary>
    private bool _wasPaused;

    /// <summary>
    /// Flag for ignoring timer initialization on lobby enter
    /// </summary>
    private bool _firstUpdate = true;

    public LobbyTimerSoundManager()
    {
        IoCManager.InjectDependencies(this);
        _audioSys = _entityManager.System<AudioSystem>();
        _lastStartTime = TimeSpan.MinValue;
        _wasPaused = false;
    }

    /// <summary>
    /// Updates the timer sound effects based on the remaining time and pause state.
    /// </summary>
    /// <param name="startTime">The target start time of the game.</param>
    /// <param name="currentTime">The current time.</param>
    /// <param name="paused">Indicates whether the game is paused.</param>
    public void Update(TimeSpan startTime, TimeSpan currentTime, bool paused)
    {
        ProcessTimeModification(startTime);
        ProcessPauseResume(paused);
        ProcessTick(startTime, currentTime, paused);
    }

    private void ProcessTimeModification(TimeSpan startTime)
    {
        if (_firstUpdate)
        {
            _lastStartTime = startTime;
            _firstUpdate = false;
            return;
        }

        if (startTime != _lastStartTime)
        {
            if (startTime > _lastStartTime)
                PlaySound(TimerSounds.Increase);
            else if (startTime < _lastStartTime)
                PlaySound(TimerSounds.Decrease);
        }
        _lastStartTime = startTime;
    }

    private void ProcessPauseResume(bool paused)
    {
        if (paused && !_wasPaused)
            PlaySound(TimerSounds.Pause);
        else if (!paused && _wasPaused)
            PlaySound(TimerSounds.Resume);
        _wasPaused = paused;
    }

    private void ProcessTick(TimeSpan startTime, TimeSpan currentTime, bool paused)
    {
        if (paused)
        {
            _lastTickSecond = -1;
            return;
        }

        var remaining = startTime - currentTime;
        if (remaining.TotalSeconds is <= 0 or > 60)
        {
            _lastTickSecond = -1;
            return;
        }

        var tickInterval = GetTickInterval(remaining.TotalSeconds);
        var secondsLeft = (int)Math.Ceiling(remaining.TotalSeconds);

        if (secondsLeft == _lastTickSecond || secondsLeft % tickInterval != 0)
            return;

        PlaySound(TimerSounds.Tick);
        _lastTickSecond = secondsLeft;
    }

    /// <summary>
    /// Determines the tick interval based on the remaining time.
    /// Rules:
    /// - 5 seconds or less: tick every 1 second.
    /// - 15 seconds or less: tick every 2 seconds.
    /// - 30 seconds or less: tick every 3 seconds.
    /// - 60 seconds or less: tick every 5 seconds.
    /// </summary>
    /// <param name="remainingSeconds">The remaining time in seconds.</param>
    /// <returns>The tick interval in seconds.</returns>
    private static int GetTickInterval(double remainingSeconds)
    {
        return remainingSeconds switch
        {
            <= 5 => 1,
            <= 15 => 2,
            <= 30 => 3,
            _ => 5,
        };
    }

    /// <summary>
    /// Plays a sound using the global audio system.
    /// </summary>
    /// <param name="soundPath">The resource path to the sound file.</param>
    private void PlaySound(ResPath soundPath)
    {
        _audioSys.PlayGlobal(soundPath.ToString(), Filter.Broadcast(), false);
    }
}
