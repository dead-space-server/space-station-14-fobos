// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Presets;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.DeadSpace.Administration.GamePreset;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Administration.GamePreset;

public sealed class GamePresetServerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IVoteManager _voteManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;

    private ISawmill _sawmill = default!;

    private readonly List<string> _activePresets = new();
    private readonly List<CustomPresetData> _customPresets = new();
    private int _maxRdmRow;
    private int _maxRdmDay;
    private int _voteDurationSeconds = 30;
    private int _currentPresetIndex;
    private int _rdmStreak;
    private bool _enabled = true;
    private bool _loaded;
    private bool _disableOocDuringVote;
    private int _activeOurVotesCount;
    private bool _originalOocEnabled;
    private bool _oocStateChangedExternally;
    private bool _ourOocChange;
    private bool _currentPresetProcessed;

    private readonly List<string> _pendingAlerts = new();
    private readonly HashSet<string> _alertedKeys = new();

    private static readonly Regex CamelCaseRegex = new("([a-z])([A-Z])", RegexOptions.Compiled);
    private static readonly Regex UpperCaseRegex = new("([A-Z]+)([A-Z][a-z])", RegexOptions.Compiled);

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("game_preset");

        SubscribeNetworkEvent<RequestGamePresetsMessage>(OnRequestPresets);
        SubscribeNetworkEvent<SetSystemEnabledMessage>(OnSetSystemEnabled);
        SubscribeNetworkEvent<CreateCustomPresetMessage>(OnCreateCustomPreset);
        SubscribeNetworkEvent<UpdateCustomPresetMessage>(OnUpdateCustomPreset);
        SubscribeNetworkEvent<DeleteCustomPresetMessage>(OnDeleteCustomPreset);
        SubscribeNetworkEvent<SetActivePresetsMessage>(OnSetActivePresets);
        SubscribeNetworkEvent<UpdatePresetSettingsMessage>(OnUpdateSettings);
        SubscribeNetworkEvent<InitiateVoteNowMessage>(OnInitiateVoteNow);
        SubscribeNetworkEvent<SkipCurrentPresetMessage>(OnSkipCurrentPreset);
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);

        _cfg.OnValueChanged(CCVars.OocEnabled, OnOocChangedExternally);

        _ = LoadFromDatabaseAsync();
    }

    private void OnOocChangedExternally(bool newValue)
    {
        if (_ourOocChange)
            return;

        if (_activeOurVotesCount > 0)
            _oocStateChangedExternally = true;
    }

    private async Task LoadFromDatabaseAsync()
    {
        try
        {
            var record = await _db.GetGamePresetConfigAsync();
            if (record != null)
            {
                _activePresets.Clear();
                _activePresets.AddRange(record.ActivePresetIds);
                _maxRdmRow = record.MaxRdmRow;
                _maxRdmDay = record.MaxRdmDay;
                _voteDurationSeconds = record.VoteDurationSeconds > 0 ? record.VoteDurationSeconds : 30;
                _currentPresetIndex = record.CurrentPresetIndex;
                _enabled = record.Enabled;
                _disableOocDuringVote = record.DisableOocDuringVote;
                _rdmStreak = 0;
                _currentPresetProcessed = false;

                if (!string.IsNullOrEmpty(record.CustomPresetsJson))
                {
                    var data = JsonSerializer.Deserialize<List<CustomPresetData>>(record.CustomPresetsJson);
                    if (data != null)
                    {
                        _customPresets.Clear();
                        _customPresets.AddRange(data);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to load game preset config from database: {ex}");
        }
        finally
        {
            _loaded = true;
        }
    }

    private async Task SaveToDatabaseAsync()
    {
        if (!_loaded)
            return;

        try
        {
            var serverId = _cfg.GetCVar(CCVars.ServerId);
            var record = new GamePresetConfigRecord
            {
                ServerId = serverId,
                Enabled = _enabled,
                ActivePresetIds = new List<string>(_activePresets),
                MaxRdmRow = _maxRdmRow,
                MaxRdmDay = _maxRdmDay,
                VoteDurationSeconds = _voteDurationSeconds,
                CurrentPresetIndex = _currentPresetIndex,
                DisableOocDuringVote = _disableOocDuringVote,
                CustomPresetsJson = JsonSerializer.Serialize(_customPresets)
            };
            await _db.UpsertGamePresetConfigAsync(record);
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to save game preset config to database: {ex}");
        }
    }

    private void OnRequestPresets(RequestGamePresetsMessage msg, EntitySessionEventArgs args)
    {
        if (!_loaded)
            return;

        SendUpdate(args.SenderSession);
    }

    private void OnSetSystemEnabled(SetSystemEnabledMessage msg, EntitySessionEventArgs args)
    {
        if (!_adminManager.HasAdminFlag(args.SenderSession, AdminFlags.Server))
            return;

        _enabled = msg.Enabled;
        SendUpdate();
        _ = SaveToDatabaseAsync();
    }

    private void OnCreateCustomPreset(CreateCustomPresetMessage msg, EntitySessionEventArgs args)
    {
        if (!_adminManager.HasAdminFlag(args.SenderSession, AdminFlags.Server))
            return;

        var id = $"custom_{_customPresets.Count}_{Guid.NewGuid():N}";
        _customPresets.Add(new CustomPresetData(id, msg.PresetName, msg.Modes, msg.PresetType, msg.Secret));
        SendUpdate();
        _ = SaveToDatabaseAsync();
    }

    private void OnUpdateCustomPreset(UpdateCustomPresetMessage msg, EntitySessionEventArgs args)
    {
        if (!_adminManager.HasAdminFlag(args.SenderSession, AdminFlags.Server))
            return;

        var index = _customPresets.FindIndex(p => p.PresetId == msg.PresetId);
        if (index < 0)
            return;

        _customPresets[index] = new CustomPresetData(msg.PresetId, msg.PresetName, msg.Modes, msg.PresetType, msg.Secret);
        SendUpdate();
        _ = SaveToDatabaseAsync();
    }

    private void OnDeleteCustomPreset(DeleteCustomPresetMessage msg, EntitySessionEventArgs args)
    {
        if (!_adminManager.HasAdminFlag(args.SenderSession, AdminFlags.Server))
            return;

        _customPresets.RemoveAll(p => p.PresetId == msg.PresetId);
        _activePresets.Remove(msg.PresetId);
        SendUpdate();
        _ = SaveToDatabaseAsync();
    }

    private void OnSetActivePresets(SetActivePresetsMessage msg, EntitySessionEventArgs args)
    {
        if (!_adminManager.HasAdminFlag(args.SenderSession, AdminFlags.Server))
            return;

        _activePresets.Clear();
        _activePresets.AddRange(msg.PresetIds);
        if (_activePresets.Count == 0)
            _currentPresetIndex = 0;
        else
            _currentPresetIndex %= _activePresets.Count;
        _currentPresetProcessed = true;
        SendUpdate();
        _ = SaveToDatabaseAsync();
    }

    private void OnUpdateSettings(UpdatePresetSettingsMessage msg, EntitySessionEventArgs args)
    {
        if (!_adminManager.HasAdminFlag(args.SenderSession, AdminFlags.Server))
            return;

        _maxRdmRow = msg.MaxRdmRow;
        _maxRdmDay = msg.MaxRdmDay;
        _voteDurationSeconds = msg.VoteDurationSeconds > 0 ? msg.VoteDurationSeconds : 30;
        _disableOocDuringVote = msg.DisableOocDuringVote;
        SendUpdate();
        _ = SaveToDatabaseAsync();
    }

    private void OnInitiateVoteNow(InitiateVoteNowMessage msg, EntitySessionEventArgs args)
    {
        if (!_adminManager.HasAdminFlag(args.SenderSession, AdminFlags.Server))
            return;

        if (_activePresets.Count == 0)
            return;

        var presetId = _activePresets[_currentPresetIndex];
        ForceVoteForPreset(presetId, manual: true);
    }

    private void OnSkipCurrentPreset(SkipCurrentPresetMessage msg, EntitySessionEventArgs args)
    {
        if (!_adminManager.HasAdminFlag(args.SenderSession, AdminFlags.Server))
            return;

        if (_activePresets.Count == 0)
            return;

        _currentPresetIndex = (_currentPresetIndex + 1) % _activePresets.Count;
        _currentPresetProcessed = true;
        _ = SaveToDatabaseAsync();
        SendUpdate();
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New == GameRunLevel.PreRoundLobby && ev.Old != GameRunLevel.PreRoundLobby)
        {
            StartVoteForNextPreset();
        }
    }

    private void StartVoteForNextPreset()
    {
        if (!_enabled)
            return;

        if (_activePresets.Count == 0)
        {
            _chatManager.SendAdminAlert(Loc.GetString("game-preset-vote-no-presets"));
            return;
        }

        if (_currentPresetProcessed)
        {
            _currentPresetIndex = (_currentPresetIndex + 1) % _activePresets.Count;
            _currentPresetProcessed = false;
            _ = SaveToDatabaseAsync();
        }

        _pendingAlerts.Clear();
        _alertedKeys.Clear();

        var found = false;
        var attempts = 0;

        while (attempts < _activePresets.Count)
        {
            var candidateId = _activePresets[_currentPresetIndex];
            var candidate = _customPresets.FirstOrDefault(p => p.PresetId == candidateId);

            if (_maxRdmRow > 0 && _rdmStreak >= _maxRdmRow)
            {
                if (candidate != null && candidate.PresetType == "rdm")
                {
                    var key = $"skip:{candidateId}";
                    if (_alertedKeys.Add(key))
                        _pendingAlerts.Add(Loc.GetString("game-preset-rdm-skipped", ("preset", candidate.PresetName)));
                    _currentPresetIndex = (_currentPresetIndex + 1) % _activePresets.Count;
                    attempts++;
                    continue;
                }

                if (candidate != null && candidate.PresetType == "democracy")
                {
                    var filtered = FilterRdmSubPresets(candidate, suppressAlerts: true);
                    if (filtered.Count == 0)
                    {
                        var key = $"skip:{candidateId}";
                        if (_alertedKeys.Add(key))
                            _pendingAlerts.Add(Loc.GetString("game-preset-democracy-all-rdm-skipped", ("preset", candidate.PresetName)));
                        _currentPresetIndex = (_currentPresetIndex + 1) % _activePresets.Count;
                        attempts++;
                        continue;
                    }
                }
            }

            found = true;
            break;
        }

        if (!found)
        {
            _pendingAlerts.Add(Loc.GetString("game-preset-rdm-limit-exceeded"));
        }

        foreach (var alert in _pendingAlerts)
        {
            _chatManager.SendAdminAlert(alert);
        }

        var presetId = _activePresets[_currentPresetIndex];
        if (ForceVoteForPreset(presetId, manual: false))
        {
            _currentPresetProcessed = true;
            _ = SaveToDatabaseAsync();
        }

        SendUpdate();
    }

    private bool IsPresetEntirelyRdm(CustomPresetData preset)
    {
        foreach (var subId in preset.Modes)
        {
            var subPreset = _customPresets.FirstOrDefault(p => p.PresetId == subId);
            if (subPreset == null)
                return false;
            if (subPreset.PresetType == "rdm")
                continue;
            if (subPreset.PresetType == "democracy" && IsPresetEntirelyRdm(subPreset))
                continue;
            return false;
        }
        return true;
    }

    private List<string> FilterRdmSubPresets(CustomPresetData democracyPreset, bool suppressAlerts = false)
    {
        var filtered = new List<string>();
        foreach (var subId in democracyPreset.Modes)
        {
            var subPreset = _customPresets.FirstOrDefault(p => p.PresetId == subId);
            if (subPreset == null)
            {
                filtered.Add(subId);
                continue;
            }

            if (subPreset.PresetType == "rdm")
            {
                if (!suppressAlerts)
                {
                    var key = $"remove:{subId}:{democracyPreset.PresetId}";
                    if (_alertedKeys.Add(key))
                        _pendingAlerts.Add(Loc.GetString("game-preset-democracy-rdm-removed", ("subpreset", subPreset.PresetName), ("parent", democracyPreset.PresetName)));
                }
                continue;
            }

            if (subPreset.PresetType == "democracy")
            {
                if (IsPresetEntirelyRdm(subPreset))
                {
                    if (!suppressAlerts)
                    {
                        var key = $"nested:{subId}:{democracyPreset.PresetId}";
                        if (_alertedKeys.Add(key))
                            _pendingAlerts.Add(Loc.GetString("game-preset-democracy-nested-all-rdm-removed", ("subpreset", subPreset.PresetName), ("parent", democracyPreset.PresetName)));
                    }
                    continue;
                }
                var nestedFiltered = FilterRdmSubPresets(subPreset, suppressAlerts);
                if (nestedFiltered.Count == 0)
                {
                    if (!suppressAlerts)
                    {
                        var key = $"nested:{subId}:{democracyPreset.PresetId}";
                        if (_alertedKeys.Add(key))
                            _pendingAlerts.Add(Loc.GetString("game-preset-democracy-nested-all-rdm-removed", ("subpreset", subPreset.PresetName), ("parent", democracyPreset.PresetName)));
                    }
                    continue;
                }
                filtered.AddRange(nestedFiltered);
                continue;
            }

            filtered.Add(subId);
        }

        return filtered;
    }

    private bool ForceVoteForPreset(string presetId, bool manual)
    {
        var preset = _customPresets.FirstOrDefault(p => p.PresetId == presetId);

        if (!manual)
        {
            if (preset != null && preset.PresetType == "rdm")
            {
                _rdmStreak++;
            }
            else if (preset == null || preset.PresetType == "calm")
            {
                _rdmStreak = 0;
            }
        }

        if (preset != null && preset.PresetType == "democracy")
        {
            var subIds = !manual && _maxRdmRow > 0 && _rdmStreak >= _maxRdmRow
                ? FilterRdmSubPresets(preset, suppressAlerts: false)
                : preset.Modes;

            if (subIds.Count == 0)
                return false;

            BeginOurVote();
            if (subIds.Count == 1)
            {
                ProcessDemocracyWinner(subIds[0], new HashSet<string>(), manual);
                return true;
            }

            StartDemocracyVote(preset, subIds, manual: manual);
            return true;
        }

        var modes = GetPresetModes(presetId);
        if (modes.Count == 0)
            return false;

        if (modes.Count == 1)
        {
            BeginOurVote();
            ApplyPreset(presetId, modes[0], announcePublic: !(preset?.Secret ?? false));
            FinishOurVotes();
            return true;
        }

        StartModeVote(presetId, modes);
        return true;
    }

    private void ApplyPreset(string presetId, string pickedMode, bool announcePublic = true)
    {
        var preset = _customPresets.FirstOrDefault(p => p.PresetId == presetId);
        var secret = preset?.Secret ?? false;

        if (secret)
        {
            _ticker.SetGamePreset(GetSecretPresetId(pickedMode));
            if (announcePublic)
                _chatManager.DispatchServerAnnouncement(Loc.GetString("game-preset-secret-win"));
            _chatManager.SendAdminAlert(Loc.GetString("game-preset-secret-win-admin", ("mode", GetModeDisplayName(pickedMode))));
        }
        else
        {
            _ticker.SetGamePreset(pickedMode);
            if (announcePublic)
                _chatManager.DispatchServerAnnouncement(
                    Loc.GetString("ui-vote-gamemode-win", ("winner", GetModeDisplayName(pickedMode))));
        }
    }

    private void StartDemocracyVote(CustomPresetData democracyPreset, List<string>? overrideSubIds = null, HashSet<string>? visited = null, bool manual = false)
    {
        visited ??= new HashSet<string>();
        if (!visited.Add(democracyPreset.PresetId))
        {
            _sawmill.Warning($"Democracy cycle detected for preset {democracyPreset.PresetId}, aborting vote.");
            return;
        }

        var subPresetIds = overrideSubIds ?? democracyPreset.Modes;
        if (subPresetIds.Count == 0)
            return;

        if (subPresetIds.Count == 1)
        {
            ProcessDemocracyWinner(subPresetIds[0], visited, manual);
            return;
        }

        var options = new VoteOptions
        {
            Title = Loc.GetString("game-preset-vote-title"),
            Duration = TimeSpan.FromSeconds(_voteDurationSeconds),
            DisplayVotes = false
        };
        options.SetInitiatorOrServer(null);

        foreach (var subId in subPresetIds)
        {
            var displayName = GetPresetDisplayName(subId);
            options.Options.Add((displayName, subId));
        }

        var vote = _voteManager.CreateVote(options);
        vote.OnFinished += (_, args) =>
        {
            if (args.Winner != null)
            {
                var winnerPresetId = args.Winner.ToString();
                if (!string.IsNullOrEmpty(winnerPresetId))
                {
                    Timer.Spawn(0, () => ProcessDemocracyWinner(winnerPresetId, visited, manual));
                }
            }
        };
    }

    private void ProcessDemocracyWinner(string winnerPresetId, HashSet<string> visited, bool manual)
    {
        var winnerPreset = _customPresets.FirstOrDefault(p => p.PresetId == winnerPresetId);
        if (winnerPreset != null && winnerPreset.PresetType == "democracy")
        {
            StartDemocracyVote(winnerPreset, null, visited, manual);
            return;
        }

        if (!manual)
        {
            if (winnerPreset != null && winnerPreset.PresetType == "rdm")
            {
                _rdmStreak++;
            }
            else
            {
                _rdmStreak = 0;
            }
        }

        var modes = GetPresetModes(winnerPresetId);
        if (modes.Count == 0)
        {
            FinishOurVotes();
            return;
        }

        if (modes.Count == 1)
        {
            ApplyPreset(winnerPresetId, modes[0], announcePublic: !(winnerPreset?.Secret ?? false));
            FinishOurVotes();
            return;
        }
        StartModeVote(winnerPresetId, modes, manageOoc: false);
    }

    private void StartModeVote(string presetId, List<string> modes, bool manageOoc = true)
    {
        if (manageOoc)
        {
            BeginOurVote();
        }
        var options = new VoteOptions
        {
            Title = Loc.GetString("game-preset-vote-title"),
            Duration = TimeSpan.FromSeconds(_voteDurationSeconds),
            DisplayVotes = false
        };
        options.SetInitiatorOrServer(null);

        foreach (var mode in modes)
        {
            var modeName = GetModeDisplayName(mode);
            options.Options.Add((modeName, mode));
        }

        var vote = _voteManager.CreateVote(options);
        vote.OnFinished += (_, args) =>
        {
            string picked;
            if (args.Winner == null)
            {
                picked = modes[new Random().Next(modes.Count)];
            }
            else
            {
                picked = (string)args.Winner;
            }
            ApplyPreset(presetId, picked, announcePublic: true);
            FinishOurVotes();
        };
    }

    private void BeginOurVote()
    {
        if (!_disableOocDuringVote)
            return;

        if (_activeOurVotesCount == 0)
        {
            _originalOocEnabled = _cfg.GetCVar(CCVars.OocEnabled);
            _oocStateChangedExternally = false;

            _ourOocChange = true;
            _cfg.SetCVar(CCVars.OocEnabled, false);
            _ourOocChange = false;
        }
        _activeOurVotesCount++;
    }

    private void FinishOurVotes()
    {
        if (!_disableOocDuringVote)
            return;

        _activeOurVotesCount--;
        if (_activeOurVotesCount == 0)
        {
            if (!_oocStateChangedExternally && _ticker.RunLevel == GameRunLevel.PreRoundLobby)
            {
                _ourOocChange = true;
                _cfg.SetCVar(CCVars.OocEnabled, _originalOocEnabled);
                _ourOocChange = false;
            }
        }
    }

    private string GetPresetDisplayName(string presetId)
    {
        var custom = _customPresets.FirstOrDefault(p => p.PresetId == presetId);
        if (custom != null)
            return custom.PresetName;

        if (_prototypeManager.TryIndex<GamePresetPrototype>(presetId, out var proto))
            return Loc.GetString(proto.ModeTitle);

        return presetId;
    }

    private string GetModeDisplayName(string modeId)
    {
        if (_prototypeManager.TryIndex<EntityPrototype>(modeId, out var entityProto))
        {
            var name = entityProto.Name;
            if (!string.IsNullOrEmpty(name))
            {
                if (name == "secret-title")
                    return Loc.GetString("secret-title") + " " + GenerateSecretPresetName(modeId);

                var localized = Loc.GetString(name);
                if (!string.IsNullOrEmpty(localized) && localized != name)
                    return localized;
            }
        }

        if (_prototypeManager.TryIndex<GamePresetPrototype>(modeId, out var presetProto))
        {
            var modeTitle = presetProto.ModeTitle;
            if (!string.IsNullOrEmpty(modeTitle))
            {
                if (modeTitle == "secret-title")
                    return Loc.GetString("secret-title") + " " + GenerateSecretPresetName(modeId);

                var localized = Loc.GetString(modeTitle);
                if (!string.IsNullOrEmpty(localized) && localized != modeTitle)
                    return localized;
            }
        }

        return modeId;
    }

    private List<string> GetPresetModes(string presetId)
    {
        var custom = _customPresets.FirstOrDefault(p => p.PresetId == presetId);
        if (custom != null)
        {
            if (custom.PresetType == "democracy")
            {
                var allModes = new List<string>();
                foreach (var subId in custom.Modes)
                {
                    allModes.AddRange(GetPresetModes(subId));
                }
                return allModes;
            }
            return custom.Modes;
        }

        if (_prototypeManager.TryIndex<GamePresetPrototype>(presetId, out var proto))
            return proto.Rules.Select(r => r.Id).ToList();

        return new List<string>();
    }

    private string GetSecretPresetId(string modeId)
    {
        var baseId = modeId;
        if (baseId.StartsWith("Secret"))
            return baseId;

        var secretId = "Secret" + baseId;
        if (_prototypeManager.TryIndex<GamePresetPrototype>(secretId, out _) ||
            _prototypeManager.TryIndex<EntityPrototype>(secretId, out _))
            return secretId;

        if (_prototypeManager.TryIndex<GamePresetPrototype>(baseId, out var proto))
        {
            foreach (var rule in proto.Rules)
            {
                if (rule.Id.StartsWith("Secret"))
                    return rule.Id;
            }
        }

        return baseId;
    }

    private void SendUpdate(ICommonSession? session = null)
    {
        var names = new Dictionary<string, string>();
        var modeNames = new Dictionary<string, string>();

        foreach (var preset in _prototypeManager.EnumeratePrototypes<GamePresetPrototype>())
        {
            if (!preset.ShowInAdminVote)
                continue;

            var name = Loc.GetString(preset.ModeTitle);
            if (preset.ModeTitle == "secret-title")
                name = Loc.GetString("secret-title") + " " + GenerateSecretPresetName(preset.ID);

            names[preset.ID] = name;

            foreach (var rule in preset.Rules)
            {
                if (!modeNames.ContainsKey(rule.Id))
                {
                    modeNames[rule.Id] = GetModeDisplayName(rule.Id);
                }
            }
        }

        foreach (var custom in _customPresets)
        {
            names[custom.PresetId] = custom.PresetName;
            foreach (var mode in custom.Modes)
            {
                if (!modeNames.ContainsKey(mode))
                    modeNames[mode] = GetModeDisplayName(mode);
            }
        }

        var response = new GamePresetsResponseMessage(
            new List<string>(_activePresets),
            new List<CustomPresetData>(_customPresets),
            names,
            _maxRdmRow,
            _maxRdmDay,
            _voteDurationSeconds,
            _currentPresetIndex,
            _enabled,
            modeNames,
            _disableOocDuringVote,
            _rdmStreak,
            _ticker.RunLevel == GameRunLevel.PreRoundLobby);

        if (session != null)
            RaiseNetworkEvent(response, session);
        else
            RaiseNetworkEvent(response, Filter.Broadcast());
    }

    private static string GenerateSecretPresetName(string id)
    {
        var name = id.Replace("Secret", "").Replace("secret", "");
        name = CamelCaseRegex.Replace(name, "$1 $2");
        name = UpperCaseRegex.Replace(name, "$1 $2");
        name = name.Trim();
        return name;
    }
}
