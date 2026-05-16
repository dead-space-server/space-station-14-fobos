// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.Administration;
using Content.Shared.DeadSpace.Administration.Events;
using Content.Shared.GameTicking;
using System.Linq;

namespace Content.Server.DeadSpace.Administration;

public sealed class RoundGameModeHistorySystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _admin = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly SecretRuleSystem _secret = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        SubscribeNetworkEvent<RoundGameModeHistoryRequestEvent>(OnHistoryRequest);
    }

    private async void OnRoundStarted(RoundStartedEvent ev)
    {
        try
        {
            var presetName = GetPresetNameForHistory();

            await _db.SetRoundGamePresetAsync(
                ev.RoundId,
                presetName);
        }
        catch (Exception e)
        {
            Log.Error($"Failed to persist round game mode history for round {ev.RoundId}: {e}");
        }
    }

    private async void OnHistoryRequest(RoundGameModeHistoryRequestEvent msg, EntitySessionEventArgs args)
    {
        if (!_admin.HasAdminFlag(args.SenderSession, AdminFlags.Round))
            return;

        try
        {
            var fromUtc = DateTime.Now.Date.AddDays(-2).ToUniversalTime();
            var today = DateTime.Now.Date;
            var rounds = await _db.GetRoundGameModeHistoryAsync(fromUtc);
            var entries = rounds
                .Select(round =>
                {
                    var localStart = round.StartDate.ToLocalTime();
                    return new RoundGameModeHistoryEntry
                    {
                        RoundId = round.RoundId,
                        DayOffset = (today - localStart.Date).Days,
                        StartedAt = localStart.ToString("dd.MM.yyyy HH:mm"),
                        GameMode = round.GamePresetName
                    };
                })
                .Where(entry => entry.DayOffset is >= 0 and <= 2)
                .ToArray();

            RaiseNetworkEvent(
                new RoundGameModeHistoryResponseEvent { Entries = entries },
                args.SenderSession.Channel);
        }
        catch (Exception e)
        {
            Log.Error($"Failed to send round game mode history: {e}");
            RaiseNetworkEvent(
                new RoundGameModeHistoryResponseEvent(),
                args.SenderSession.Channel);
        }
    }

    private string? GetPresetNameForHistory()
    {
        var preset = _ticker.CurrentPreset;
        if (preset == null)
            return null;

        var presetName = Loc.GetString(preset.ModeTitle);
        if (!string.Equals(preset.ID, "Secret", StringComparison.OrdinalIgnoreCase))
            return presetName;

        var selectedSecretPreset = _secret.SelectedSecretPreset;
        if (selectedSecretPreset == null)
            return presetName;

        return Loc.GetString(
            "round-game-mode-history-secret-format",
            ("secret", presetName),
            ("selected", Loc.GetString(selectedSecretPreset.ModeTitle)));
    }
}
