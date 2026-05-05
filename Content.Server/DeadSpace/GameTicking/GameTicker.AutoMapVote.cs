// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    /// <summary>
    /// Returns the remaining lobby window where the next round's map may still be changed.
    /// </summary>
    public TimeSpan TimeUntilMapChangeCloses()
    {
        if (RunLevel != GameRunLevel.PreRoundLobby)
            return TimeSpan.Zero;

        // PreRound is raised before GameTicker always finishes initializing the lobby countdown.
        // Treat the countdown as still open until the timer is actually set.
        if (_roundStartTime == TimeSpan.Zero)
            return TimeSpan.MaxValue;

        var referenceTime = Paused && _pauseTime != TimeSpan.Zero
            ? _pauseTime
            : _gameTiming.CurTime;

        return _roundStartTime - RoundPreloadTime - referenceTime;
    }
}
