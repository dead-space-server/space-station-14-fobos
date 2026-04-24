// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.DeadSpace.Maps;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared.DeadSpace.Administration.Events;
using Content.Shared.CCVar;
using Content.Shared.Maps;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Voting;

public sealed class AutoMapVoteSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IGameMapManager _gameMapManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IVoteManager _voteManager = default!;

    private readonly Dictionary<AutoMapVoteCategory, AutoMapVoteCategoryConfig> _configs = new();
    private readonly Dictionary<AutoMapVoteCategory, HashSet<string>> _playedMaps = new();
    private readonly HashSet<string> _blacklistedMaps = new(StringComparer.Ordinal);

    private IVoteHandle? _activeVote;
    private AutoMapVoteCategory? _activeVoteCategory;
    private TimeSpan? _activeVoteEndTime;
    private string _blacklistMapsCsv = string.Empty;
    private bool _enabled;
    private bool? _lastReportedVoteActive;
    private bool? _lastReportedVoteBlocked;
    private int _lastHandledRoundId = -1;
    private int _voteDurationSeconds = 90;

    public override void Initialize()
    {
        base.Initialize();

        foreach (var category in Enum.GetValues<AutoMapVoteCategory>())
        {
            _configs[category] = new AutoMapVoteCategoryConfig();
            _playedMaps[category] = new HashSet<string>(StringComparer.Ordinal);
        }

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChanged);

        Subs.CVar(_config, CCVars.VoteAutoMapEnabled, OnEnabledChanged, true);
        Subs.CVar(_config, CCVars.VoteAutoMapSmallMaxPlayers, value => UpdateCategoryMaxPlayers(AutoMapVoteCategory.Small, value), true);
        Subs.CVar(_config, CCVars.VoteAutoMapMediumMaxPlayers, value => UpdateCategoryMaxPlayers(AutoMapVoteCategory.Medium, value), true);
        Subs.CVar(_config, CCVars.VoteAutoMapLargeMaxPlayers, value => UpdateCategoryMaxPlayers(AutoMapVoteCategory.Large, value), true);
        Subs.CVar(_config, CCVars.VoteAutoMapSmallMaps, value => UpdateCategoryMaps(AutoMapVoteCategory.Small, value), true);
        Subs.CVar(_config, CCVars.VoteAutoMapMediumMaps, value => UpdateCategoryMaps(AutoMapVoteCategory.Medium, value), true);
        Subs.CVar(_config, CCVars.VoteAutoMapLargeMaps, value => UpdateCategoryMaps(AutoMapVoteCategory.Large, value), true);
        Subs.CVar(_config, CCVars.VoteAutoMapBlacklistMaps, UpdateBlacklistMaps, true);
        Subs.CVar(_config, CCVars.VoteAutoMapDuration, OnVoteDurationChanged, true);

        _adminManager.OnPermsChanged += OnAdminPermsChanged;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_activeVote is not { Finished: false } vote || _activeVoteCategory == null)
        {
            UpdateDerivedAdminState();
            return;
        }

        if (_activeVoteEndTime == null || _timing.CurTime < _activeVoteEndTime.Value)
        {
            UpdateDerivedAdminState();
            return;
        }

        FinishExpiredVote(vote, _activeVoteCategory.Value);
        UpdateDerivedAdminState();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _adminManager.OnPermsChanged -= OnAdminPermsChanged;
    }

    public bool Enabled => _enabled;

    public AutoMapVoteAdminState GetAdminState()
    {
        var availableIds = _gameMapManager
            .CurrentlyEligibleMaps()
            .Select(map => map.ID)
            .Where(id => !_blacklistedMaps.Contains(id))
            .ToHashSet(StringComparer.Ordinal);

        var availableMaps = _gameMapManager
            .AllMaps()
            .OrderBy(map => map.MapName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(map => map.ID, StringComparer.Ordinal)
            .Select(map => new AutoMapVoteMapEntry
            {
                Id = map.ID,
                Name = map.MapName,
                EligibleNow = availableIds.Contains(map.ID),
                Blacklisted = _blacklistedMaps.Contains(map.ID)
            })
            .ToArray();

        return new AutoMapVoteAdminState
        {
            Enabled = _enabled,
            VoteActive = HasActiveVote(),
            VoteBlocked = IsVoteBlocked(),
            CurrentPlayerCount = _playerManager.PlayerCount,
            CurrentCategory = ResolveCategory(_playerManager.PlayerCount),
            VoteDurationSeconds = _voteDurationSeconds,
            AvailableMaps = availableMaps,
            Categories =
            [
                BuildCategoryStatus(AutoMapVoteCategory.Small, availableIds),
                BuildCategoryStatus(AutoMapVoteCategory.Medium, availableIds),
                BuildCategoryStatus(AutoMapVoteCategory.Large, availableIds)
            ],
            Blacklist = BuildBlacklistStatus()
        };
    }

    public void SetEnabled(bool enabled, bool save = true)
    {
        _config.SetCVar(CCVars.VoteAutoMapEnabled, enabled);

        if (save)
            _config.SaveToFile();
    }

    public bool TryInitiateVote([NotNullWhen(false)] out string? error)
    {
        error = null;

        if (_gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
        {
            error = Loc.GetString("auto-map-vote-initiate-error-not-lobby");
            return false;
        }

        if (_activeVote is { Finished: false })
        {
            error = Loc.GetString("auto-map-vote-initiate-error-already-running");
            return false;
        }

        if (!CanInitiateVote(out error))
            return false;

        _lastHandledRoundId = _gameTicker.RoundId;
        StartAutoMapVoteCycleCore();
        SendAdminState();
        return true;
    }

    public bool TryApplyConfiguration(
        int smallMaxPlayers,
        int mediumMaxPlayers,
        int largeMaxPlayers,
        string smallMaps,
        string mediumMaps,
        string largeMaps,
        string blacklistMaps,
        int? voteDurationSeconds,
        [NotNullWhen(false)] out string? error)
    {
        error = null;

        if (smallMaxPlayers < 0 || mediumMaxPlayers < 0 || largeMaxPlayers < 0)
        {
            error = Loc.GetString("auto-map-vote-config-error-negative-player-count");
            return false;
        }

        if (voteDurationSeconds != null && voteDurationSeconds.Value <= 0)
        {
            error = Loc.GetString("auto-map-vote-config-error-invalid-duration");
            return false;
        }

        var normalizedBlacklist = NormalizeMapsCsv(blacklistMaps);
        var blacklistIds = ParseMapIds(normalizedBlacklist).ToHashSet(StringComparer.Ordinal);

        _config.SetCVar(CCVars.VoteAutoMapSmallMaxPlayers, smallMaxPlayers);
        _config.SetCVar(CCVars.VoteAutoMapMediumMaxPlayers, mediumMaxPlayers);
        _config.SetCVar(CCVars.VoteAutoMapLargeMaxPlayers, largeMaxPlayers);
        _config.SetCVar(CCVars.VoteAutoMapBlacklistMaps, normalizedBlacklist);
        _config.SetCVar(CCVars.VoteAutoMapSmallMaps, NormalizeMapsCsv(smallMaps, blacklistIds));
        _config.SetCVar(CCVars.VoteAutoMapMediumMaps, NormalizeMapsCsv(mediumMaps, blacklistIds));
        _config.SetCVar(CCVars.VoteAutoMapLargeMaps, NormalizeMapsCsv(largeMaps, blacklistIds));

        if (voteDurationSeconds != null)
            _config.SetCVar(CCVars.VoteAutoMapDuration, voteDurationSeconds.Value);

        _config.SaveToFile();
        SendAdminState();
        return true;
    }

    private void OnEnabledChanged(bool value)
    {
        _enabled = value;

        if (!value)
        {
            CancelActiveVote();
            _gameMapManager.EndAutoMapVoteOverride();
            _gameTicker.UpdateInfoText();
        }

        SendAdminState();
    }

    private void UpdateCategoryMaxPlayers(AutoMapVoteCategory category, int value)
    {
        _configs[category].MaxPlayers = value;
        SendAdminState();
    }

    private void UpdateCategoryMaps(AutoMapVoteCategory category, string value)
    {
        _configs[category].MapsCsv = NormalizeMapsCsv(value, _blacklistedMaps);
        SendAdminState();
    }

    private void UpdateBlacklistMaps(string value)
    {
        _blacklistMapsCsv = NormalizeMapsCsv(value);
        _blacklistedMaps.Clear();

        foreach (var id in ParseMapIds(_blacklistMapsCsv))
        {
            _blacklistedMaps.Add(id);
        }

        SendAdminState();
    }

    private void OnVoteDurationChanged(int value)
    {
        _voteDurationSeconds = value;
        SendAdminState();
    }

    private void OnAdminPermsChanged(AdminPermsChangedEventArgs args)
    {
        if (!args.IsAdmin)
            return;

        SendAdminState(args.Player);
    }

    private void OnGameRunLevelChanged(GameRunLevelChangedEvent args)
    {
        if (args.New == GameRunLevel.PreRoundLobby && args.Old != GameRunLevel.PreRoundLobby)
        {
            if (!_enabled)
                return;

            Timer.Spawn(0, StartAutoMapVoteCycle);
            return;
        }

        if (args.New != GameRunLevel.PreRoundLobby)
        {
            CancelActiveVote();
            _gameMapManager.EndAutoMapVoteOverride();
            _gameTicker.UpdateInfoText();
        }
    }

    private void StartAutoMapVoteCycle()
    {
        if (!_enabled || _gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
            return;

        if (_lastHandledRoundId == _gameTicker.RoundId)
            return;

        _lastHandledRoundId = _gameTicker.RoundId;
        StartAutoMapVoteCycleCore();
    }

    private void StartAutoMapVoteCycleCore()
    {
        CancelActiveVote();

        if (IsVoteBlocked())
            return;

        _gameMapManager.BeginAutoMapVoteOverride();
        _gameTicker.UpdateInfoText();

        var category = ResolveCategory(_playerManager.PlayerCount);
        var candidates = BuildCandidatePool(category);

        if (candidates.Count == 0)
        {
            ApplyDefaultSelection();
            return;
        }

        var voteDuration = GetVoteDuration();
        var forceWithoutVote =
            candidates.Count == 1 ||
            _playerManager.PlayerCount == 0;

        if (forceWithoutVote)
        {
            var picked = candidates.Count == 1
                ? candidates[0]
                : _random.Pick(candidates);

            ApplySelectedMap(category, picked, announceImmediate: true);
            return;
        }

        CreateAutoVote(category, candidates, voteDuration);
    }

    private void CreateAutoVote(AutoMapVoteCategory category, List<GameMapPrototype> candidates, TimeSpan duration)
    {
        var options = new VoteOptions
        {
            Title = Loc.GetString("ui-vote-map-title"),
            Duration = duration
        };

        options.SetInitiatorOrServer(null);

        foreach (var map in candidates)
        {
            options.Options.Add((map.MapName, map));
        }

        _activeVote = _voteManager.CreateVote(options);
        _activeVoteCategory = category;
        _activeVoteEndTime = _timing.CurTime + duration;
        _activeVote.OnFinished += OnAutoVoteFinished;
        _activeVote.OnCancelled += OnAutoVoteCancelled;
        SendAdminState();
    }

    private void OnAutoVoteFinished(IVoteHandle sender, VoteFinishedEventArgs args)
    {
        if (sender != _activeVote || _activeVoteCategory == null)
            return;

        var category = _activeVoteCategory.Value;
        ClearActiveVoteState();
        SendAdminState();

        if (_gameTicker.RunLevel != GameRunLevel.PreRoundLobby || !_gameTicker.CanUpdateMap())
            return;

        var picked = ResolveVoteWinner(args);
        if (picked == null)
            return;

        AnnounceVoteResult(args, picked);
        ApplySelectedMap(category, picked, announceImmediate: false);
    }

    private void OnAutoVoteCancelled(IVoteHandle sender)
    {
        if (sender != _activeVote)
            return;

        ClearActiveVoteState();
        SendAdminState();

        if (_gameTicker.RunLevel != GameRunLevel.PreRoundLobby || !_gameTicker.CanUpdateMap())
            return;

        if (_gameMapManager.GetSelectedMap() != null)
            return;

        ApplyDefaultSelection();
    }

    private void FinishExpiredVote(IVoteHandle vote, AutoMapVoteCategory category)
    {
        var picked = ResolveVoteWinner(vote);
        if (picked == null)
            return;

        CancelActiveVote();

        if (!_gameTicker.CanUpdateMap())
            return;

        AnnounceVoteResult(vote, picked);
        ApplySelectedMap(category, picked, announceImmediate: false);
    }

    private void ApplyDefaultSelection()
    {
        if (!_gameTicker.CanUpdateMap())
            return;

        var fallbackPool = _gameMapManager
            .CurrentlyEligibleMaps()
            .Where(map => !_blacklistedMaps.Contains(map.ID))
            .ToList();

        if (fallbackPool.Count > 0)
        {
            var picked = _random.Pick(fallbackPool);
            _gameMapManager.SelectMap(picked.ID, MapSelectionContext.AutoMapVote);
            _gameTicker.UpdateInfoText();
            _chatManager.DispatchServerAnnouncement(Loc.GetString("auto-map-vote-fallback-selection"));
            return;
        }

        _gameMapManager.SelectMapByConfigRules(MapSelectionContext.AutoMapVote);
        _gameTicker.UpdateInfoText();
        _chatManager.DispatchServerAnnouncement(Loc.GetString("auto-map-vote-fallback-selection"));
    }

    private void ApplySelectedMap(AutoMapVoteCategory category, GameMapPrototype map, bool announceImmediate)
    {
        if (!_gameTicker.CanUpdateMap())
            return;

        if (!_gameMapManager.TrySelectMapIfEligible(map.ID, MapSelectionContext.AutoMapVote))
        {
            ApplyDefaultSelection();
            return;
        }

        _playedMaps[category].Add(map.ID);
        _gameTicker.UpdateInfoText();

        if (announceImmediate)
        {
            _chatManager.DispatchServerAnnouncement(Loc.GetString("auto-map-vote-selected-immediately", ("winner", map.MapName)));
        }
    }

    private List<GameMapPrototype> BuildCandidatePool(AutoMapVoteCategory category)
    {
        var basePool = BuildConfiguredEligiblePool(category);
        if (basePool.Count == 0)
            return basePool;

        var played = _playedMaps[category];
        var available = basePool
            .Where(map => !played.Contains(map.ID))
            .ToList();

        if (available.Count != 0)
            return available;

        played.Clear();
        return basePool;
    }

    private List<GameMapPrototype> BuildConfiguredEligiblePool(AutoMapVoteCategory category)
    {
        var configuredMaps = _gameMapManager
            .CurrentlyEligibleMaps()
            .ToDictionary(map => map.ID, map => map, StringComparer.Ordinal);

        var result = new List<GameMapPrototype>();
        foreach (var id in ParseMapIds(_configs[category].MapsCsv))
        {
            if (_blacklistedMaps.Contains(id))
                continue;

            if (configuredMaps.TryGetValue(id, out var map))
                result.Add(map);
        }

        return result;
    }

    private GameMapPrototype? ResolveVoteWinner(IVoteHandle vote)
    {
        var winners = vote.VotesPerOption
            .GroupBy(entry => entry.Value)
            .OrderByDescending(group => group.Key)
            .FirstOrDefault()?
            .Select(entry => (GameMapPrototype) entry.Key)
            .ToArray();

        if (winners == null || winners.Length == 0)
            return null;

        return winners.Length == 1
            ? winners[0]
            : _random.Pick(winners);
    }

    private GameMapPrototype? ResolveVoteWinner(VoteFinishedEventArgs args)
    {
        if (args.Winner != null)
            return (GameMapPrototype) args.Winner;

        return args.Winners.Length == 0
            ? null
            : (GameMapPrototype) _random.Pick(args.Winners);
    }

    private void AnnounceVoteResult(VoteFinishedEventArgs args, GameMapPrototype picked)
    {
        if (args.Winner == null)
        {
            _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-map-tie", ("picked", picked.MapName)));
            return;
        }

        _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-map-win", ("winner", picked.MapName)));
    }

    private void AnnounceVoteResult(IVoteHandle vote, GameMapPrototype picked)
    {
        var maxVotes = vote.VotesPerOption.Values.DefaultIfEmpty(0).Max();
        var winners = vote.VotesPerOption.Count(entry => entry.Value == maxVotes);

        if (winners > 1)
        {
            _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-map-tie", ("picked", picked.MapName)));
            return;
        }

        _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-map-win", ("winner", picked.MapName)));
    }

    private AutoMapVoteCategoryStatus BuildCategoryStatus(AutoMapVoteCategory category, HashSet<string> availableIds)
    {
        var config = _configs[category];

        var unknown = new List<string>();
        var validCount = 0;
        var eligibleCount = 0;
        var configuredIds = ParseMapIds(config.MapsCsv);

        foreach (var id in configuredIds)
        {
            if (!_prototypeManager.TryIndex<GameMapPrototype>(id, out _) || _blacklistedMaps.Contains(id))
            {
                if (!_blacklistedMaps.Contains(id))
                    unknown.Add(id);
                continue;
            }

            validCount++;
            if (availableIds.Contains(id))
                eligibleCount++;
        }

        return new AutoMapVoteCategoryStatus
        {
            Category = category,
            MaxPlayers = config.MaxPlayers,
            MapsCsv = config.MapsCsv,
            ConfiguredMapCount = configuredIds.Count,
            ValidMapCount = validCount,
            EligibleMapCount = eligibleCount,
            UnknownMaps = unknown.ToArray()
        };
    }

    private AutoMapVoteBlacklistStatus BuildBlacklistStatus()
    {
        var unknown = new List<string>();
        var validCount = 0;
        var configuredIds = ParseMapIds(_blacklistMapsCsv);

        foreach (var id in configuredIds)
        {
            if (!_prototypeManager.TryIndex<GameMapPrototype>(id, out _))
            {
                unknown.Add(id);
                continue;
            }

            validCount++;
        }

        return new AutoMapVoteBlacklistStatus
        {
            MapsCsv = _blacklistMapsCsv,
            ConfiguredMapCount = configuredIds.Count,
            ValidMapCount = validCount,
            UnknownMaps = unknown.ToArray()
        };
    }

    private AutoMapVoteCategory ResolveCategory(int playerCount)
    {
        if (playerCount <= _configs[AutoMapVoteCategory.Small].MaxPlayers)
            return AutoMapVoteCategory.Small;

        if (playerCount <= _configs[AutoMapVoteCategory.Medium].MaxPlayers)
            return AutoMapVoteCategory.Medium;

        return AutoMapVoteCategory.Large;
    }

    private TimeSpan GetVoteDuration()
    {
        return TimeSpan.FromSeconds(_voteDurationSeconds);
    }

    private bool CanInitiateVote([NotNullWhen(false)] out string? error)
    {
        error = null;

        if (!_gameTicker.CanUpdateMap())
        {
            error = Loc.GetString("auto-map-vote-initiate-error-map-update-closed");
            return false;
        }

        if (_gameTicker.TimeUntilMapChangeCloses() <= GetVoteDuration())
        {
            error = Loc.GetString("auto-map-vote-initiate-error-blocked");
            return false;
        }

        return true;
    }

    private bool HasActiveVote()
    {
        return _activeVote is { Finished: false };
    }

    private bool IsVoteBlocked()
    {
        if (HasActiveVote())
            return false;

        if (_gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
            return true;

        if (!_gameTicker.CanUpdateMap())
            return true;

        return _gameTicker.TimeUntilMapChangeCloses() <= GetVoteDuration();
    }

    private void UpdateDerivedAdminState()
    {
        var voteActive = HasActiveVote();
        var voteBlocked = IsVoteBlocked();

        if (_lastReportedVoteActive == voteActive && _lastReportedVoteBlocked == voteBlocked)
            return;

        SendAdminState();
    }

    private void CancelActiveVote()
    {
        if (_activeVote is not { Finished: false } vote)
        {
            ClearActiveVoteState();
            return;
        }

        vote.OnFinished -= OnAutoVoteFinished;
        vote.OnCancelled -= OnAutoVoteCancelled;
        vote.Cancel();
        ClearActiveVoteState();
    }

    public void OnForcedMapSelected()
    {
        CancelActiveVote();
        _gameTicker.UpdateInfoText();
    }

    public void OnForcedMapCleared()
    {
        _gameTicker.UpdateInfoText();
    }

    private void ClearActiveVoteState()
    {
        _activeVote = null;
        _activeVoteCategory = null;
        _activeVoteEndTime = null;
    }

    private void SendAdminState(ICommonSession? player = null)
    {
        _lastReportedVoteActive = HasActiveVote();
        _lastReportedVoteBlocked = IsVoteBlocked();
        var ev = new AutoMapVoteAdminStateChangedEvent(GetAdminState());

        if (player != null)
        {
            RaiseNetworkEvent(ev, player.Channel);
            return;
        }

        foreach (var admin in _adminManager.AllAdmins)
        {
            RaiseNetworkEvent(ev, admin);
        }
    }

    private static string NormalizeMapsCsv(string csv, HashSet<string>? excludedIds = null)
    {
        var ids = ParseMapIds(csv);

        if (excludedIds != null && excludedIds.Count > 0)
            ids.RemoveAll(excludedIds.Contains);

        return string.Join(", ", ids);
    }

    private static List<string> ParseMapIds(string csv)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in csv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (!seen.Add(entry))
                continue;

            result.Add(entry);
        }

        return result;
    }

    private sealed class AutoMapVoteCategoryConfig
    {
        public int MaxPlayers;
        public string MapsCsv = string.Empty;
    }
}
