using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using Content.Server.Administration.Managers;
using Content.Server.Database;
using Robust.Server.ServerStatus;

namespace Content.Server.Administration;

public sealed partial class ServerApi
{
    private const int DefaultRoundStatsDays = 7;
    private const int MaxRoundStatsDays = 365;

    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ServerDbEntryManager _serverDbEntry = default!;

    /// <summary>
    /// Get players and active admins list
    /// </summary>
    private async Task GetPlayers(IStatusHandlerContext context)
    {
        var playersList = new JsonArray();
        foreach (var player in _playerManager.Sessions)
        {
            playersList.Add(player.Name);
        }

        var adminMgr = await RunOnMainThread(IoCManager.Resolve<IAdminManager>);
        var adminsDict = new JsonObject();

        foreach (var admin in adminMgr.AllAdmins)
        {
            var adminData = adminMgr.GetAdminData(admin, true)!;
            adminsDict[admin.Name] = new JsonObject
            {
                ["isActive"] = adminData.Active,
                ["isStealth"] = adminData.Stealth,
                ["title"] = adminData.Title,
                ["flags"] = JsonSerializer.SerializeToNode(adminData.Flags.ToString().Split(", ")),
            };
        }

        var jObject = new JsonObject
        {
            ["players"] = playersList,
            ["admins"] = adminsDict
        };

        await context.RespondJsonAsync(jObject);
    }

    private async Task GetRoundStats(IStatusHandlerContext context)
    {
        var query = HttpUtility.ParseQueryString(context.Url.Query);
        if (!TryResolveRoundStatsPeriod(query, out var fromUtc, out var toUtc, out var error))
        {
            await RespondBadRequest(context, error);
            return;
        }

        var server = await _serverDbEntry.ServerEntity;
        var rounds = await _db.GetRoundGameModeHistoryAsync(server.Id, fromUtc);
        rounds = rounds
            .Where(round => round.StartDate < toUtc)
            .OrderByDescending(round => round.StartDate)
            .ToList();

        var response = new RoundStatsResponse
        {
            Server = server.Name,
            From = fromUtc,
            To = toUtc,
            TotalRounds = rounds.Count,
            Modes = AggregateRoundStats(rounds.Select(round => NormalizeRoundStatsName(round.GamePresetName)), rounds.Count),
            Maps = AggregateRoundStats(rounds.Select(round => NormalizeRoundStatsName(round.MapName)), rounds.Count),
            Rounds = rounds
                .Select(round => new RoundStatsResponse.Round
                {
                    RoundId = round.RoundId,
                    StartedAt = round.StartDate,
                    GameMode = NormalizeRoundStatsName(round.GamePresetName),
                    Map = NormalizeRoundStatsName(round.MapName),
                    PlayerCount = round.PlayerCount
                })
                .ToArray()
        };

        await context.RespondJsonAsync(response);
    }

    private static bool TryResolveRoundStatsPeriod(
        System.Collections.Specialized.NameValueCollection query,
        out DateTime fromUtc,
        out DateTime toUtc,
        out string error)
    {
        error = string.Empty;
        toUtc = DateTime.UtcNow;

        if (!TryParseRoundStatsDate(query["to"], out var parsedTo))
        {
            fromUtc = default;
            error = "Invalid 'to' value";
            return false;
        }

        if (parsedTo != null)
            toUtc = parsedTo.Value;

        if (!TryParseRoundStatsDate(query["from"], out var parsedFrom))
        {
            fromUtc = default;
            error = "Invalid 'from' value";
            return false;
        }

        if (parsedFrom != null)
        {
            fromUtc = parsedFrom.Value;
        }
        else
        {
            if (!TryResolveRoundStatsDays(query["period"], query["days"], out var days, out error))
            {
                fromUtc = default;
                return false;
            }

            fromUtc = toUtc.AddDays(-days);
        }

        if (fromUtc >= toUtc)
        {
            error = "'from' must be earlier than 'to'";
            return false;
        }

        if (toUtc - fromUtc > TimeSpan.FromDays(MaxRoundStatsDays))
        {
            error = $"Period cannot exceed {MaxRoundStatsDays} days";
            return false;
        }

        return true;
    }

    private static bool TryResolveRoundStatsDays(string? period, string? daysText, out int days, out string error)
    {
        error = string.Empty;
        days = DefaultRoundStatsDays;

        if (!string.IsNullOrWhiteSpace(period))
        {
            days = period.Trim().ToLowerInvariant() switch
            {
                "day" or "daily" or "today" => 1,
                "week" or "weekly" => 7,
                "month" or "monthly" => 30,
                _ => -1
            };

            if (days == -1)
            {
                error = "Invalid 'period' value";
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(daysText) && !int.TryParse(daysText, out days))
        {
            error = "Invalid 'days' value";
            return false;
        }

        if (days is < 1 or > MaxRoundStatsDays)
        {
            error = $"'days' must be between 1 and {MaxRoundStatsDays}";
            return false;
        }

        return true;
    }

    private static bool TryParseRoundStatsDate(string? value, out DateTime? date)
    {
        date = null;
        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (!DateTime.TryParse(
                value,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return false;
        }

        date = parsed.ToUniversalTime();
        return true;
    }

    private static RoundStatsResponse.Stat[] AggregateRoundStats(IEnumerable<string> values, int total)
    {
        return values
            .GroupBy(value => value)
            .Select(group => new RoundStatsResponse.Stat
            {
                Name = group.Key,
                Count = group.Count(),
                Percent = total == 0 ? 0 : Math.Round(group.Count() * 100d / total, 2)
            })
            .OrderByDescending(stat => stat.Count)
            .ThenBy(stat => stat.Name)
            .ToArray();
    }

    private static string NormalizeRoundStatsName(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
    }

    private sealed class RoundStatsResponse
    {
        public required string Server { get; init; }
        public required DateTime From { get; init; }
        public required DateTime To { get; init; }
        public required int TotalRounds { get; init; }
        public required Stat[] Modes { get; init; }
        public required Stat[] Maps { get; init; }
        public required Round[] Rounds { get; init; }

        public sealed class Stat
        {
            public required string Name { get; init; }
            public required int Count { get; init; }
            public required double Percent { get; init; }
        }

        public sealed class Round
        {
            public required int RoundId { get; init; }
            public required DateTime StartedAt { get; init; }
            public required string GameMode { get; init; }
            public required string Map { get; init; }
            public required int? PlayerCount { get; init; }
        }
    }
}
