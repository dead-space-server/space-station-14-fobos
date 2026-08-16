using System.Linq;
using Content.Server.Antag.Components;
using Content.Server.Chat.Managers;
using Content.Server.Doors.Systems;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.Hands.Systems;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.DeadSpace.Prison;
using Content.Server.Stunnable;
using Content.Shared.Armor;
using Content.Shared.Body.Part;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Doors.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Station;
using Content.Shared.Storage;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Store;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.DeadSpace.Arena;

public sealed class ArenaSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _protos = default!;
    [Dependency] private readonly ITileDefinitionManager _tiles = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly MindSystem _minds = default!;
    [Dependency] private readonly GhostSystem _ghosts = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly IRobustRandom _luck = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly SharedStationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly PrisonSystem _prison = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedArmorSystem _armor = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly DoorSystem _doorSystem = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private const string ArenaMapFile = "/Maps/_DeadSpace/arena.yml";

    private const float DeathmatchDuration = 600f;
    private const float TDMPreparationDuration = 30f;
    private const float TDMRoundDuration = 600f;
    private const float IntermissionDuration = 25f;

    /// <summary>Бюджет закупки в TDM (в ТК).</summary>
    private const int TdmStoreBudget = 40;

    public bool Enabled { get; private set; } = true;

    public void ToggleEnabled()
    {
        Enabled = !Enabled;
    }

    public ArenaMode CurrentMode { get; private set; } = ArenaMode.Deathmatch;
    public ArenaMode NextMode { get; set; } = ArenaMode.Deathmatch;
    public ArenaRoundState RoundState { get; private set; } = ArenaRoundState.Intermission;
    public float RoundTimeRemaining { get; private set; } = IntermissionDuration;
    public bool RoundStarted { get; private set; }

    private EntityUid? _arenaMap;
    private readonly HashSet<NetEntity> _roster = new();
    private readonly Dictionary<NetEntity, ArenaMode> _votes = new();
    private readonly Dictionary<NetEntity, ArenaTeam> _playerTeams = new();
    private readonly List<EntityCoordinates> _blueSpawns = new();
    private readonly List<EntityCoordinates> _redSpawns = new();
    private readonly List<EntityUid> _tdmDoors = new();
    private readonly List<ArenaLoadoutPresetPrototype> _presets = new();
    private readonly List<ArenaCostumePrototype> _costumes = new();
    private readonly Dictionary<ICommonSession, ArenaLoadoutEui> _activeEuis = new();

    private readonly Dictionary<NetUserId, int> _killCurrency = new();
    private readonly Dictionary<NetUserId, HashSet<string>> _ownedCostumes = new();
    private readonly Dictionary<NetUserId, List<string>> _equippedCostumes = new();
    private readonly Dictionary<NetUserId, ArenaPlayerRecord> _records = new();

    // Per-round Deathmatch K/D
    private readonly Dictionary<NetUserId, int> _dmKills = new();
    private readonly Dictionary<NetUserId, int> _dmDeaths = new();
    // TDM score per team
    private readonly Dictionary<ArenaTeam, int> _tdmTeamKills = new()
    {
        [ArenaTeam.Blue] = 0,
        [ArenaTeam.Red] = 0,
    };
    // Lock players to their first chosen team for the current TDM round
    private readonly Dictionary<NetUserId, ArenaTeam> _tdmTeamLocks = new();
    // TDM pre-round purchases (listing IDs chosen from the store tab)
    private readonly Dictionary<NetUserId, List<string>> _tdmPurchases = new();
    // Persistent stats across sub-rounds (not cleared between DM/TDM rounds)
    private readonly Dictionary<NetUserId, int> _persistDmKills = new();
    private readonly Dictionary<NetUserId, int> _persistDmDeaths = new();
    private readonly Dictionary<NetUserId, int> _persistTdmKills = new();
    private readonly Dictionary<NetUserId, int> _persistTdmDeaths = new();
    private readonly Dictionary<NetUserId, string> _persistPlayerNames = new();
    private int _persistTdmBlueKills;
    private int _persistTdmRedKills;
    // Тела участников TDM, которые сами вышли в гост из критического состояния (команда ghost).
    // Возрождать их не нужно: игрок уже ушёл в гост, новое тело не спавнится.
    private readonly HashSet<NetEntity> _ghostOutRequests = new();

    private float _broadcastTimer;
    private float _cleanTick;

    internal bool CanJoinArena(ICommonSession session)
    {
        return Enabled && !_prison.IsUserPrisoner(session.UserId);
    }

    public override void Initialize()
    {
        SubscribeNetworkEvent<ArenaJoinEvent>(OnJoin);
        SubscribeNetworkEvent<ArenaLeaveEvent>(OnLeave);
        SubscribeNetworkEvent<ArenaVoteCastEvent>(OnVoteCast);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<PrisonerRegisteredEvent>(OnPrisonerRegistered);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
        SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhostAttempt);
    }

    private void RefreshPresets()
    {
        _presets.Clear();
        foreach (var p in _protos.EnumeratePrototypes<ArenaLoadoutPresetPrototype>())
            _presets.Add(p);

        _costumes.Clear();
        foreach (var c in _protos.EnumeratePrototypes<ArenaCostumePrototype>())
            _costumes.Add(c);
    }

    private void OnJoin(ArenaJoinEvent msg, EntitySessionEventArgs args)
    {
        var who = (ICommonSession)args.SenderSession;

        if (!CanJoinArena(who))
        {
            if (_prison.IsUserPrisoner(who.UserId))
                _chat.DispatchServerMessage(who, Loc.GetString("prison-arena-blocked"));
            return;
        }

        if (who.AttachedEntity is not { Valid: true } ghost || !HasComp<GhostComponent>(ghost))
            return;

        if (_activeEuis.ContainsKey(who))
            return;

        if (_presets.Count == 0)
            RefreshPresets();

        var eui = new ArenaLoadoutEui(this, who, ghost);
        _eui.OpenEui(eui, who);
        _activeEuis[who] = eui;
    }

    private void OnPrisonerRegistered(ref PrisonerRegisteredEvent ev)
    {
        if (_activeEuis.TryGetValue(ev.Session, out var eui) && !eui.IsShutDown)
            eui.Close();

        if (ev.Session.AttachedEntity is { Valid: true } body &&
            TryComp<ArenaPlayerComponent>(body, out var arenaPlayer) &&
            _roster.Contains(GetNetEntity(body)))
        {
            RestorePlayer(body, arenaPlayer);
        }
    }

    private void OnLeave(ArenaLeaveEvent msg, EntitySessionEventArgs args)
    {
        var who = (ICommonSession)args.SenderSession;
        if (who.AttachedEntity is not { Valid: true } body ||
            !TryComp<ArenaPlayerComponent>(body, out var arenaPlayer) ||
            !_roster.Contains(GetNetEntity(body)))
            return;

        _playerTeams.Remove(GetNetEntity(body));
        RestorePlayer(body, arenaPlayer);
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        if (!TryComp<ArenaPlayerComponent>(ev.Target, out var arenaPlayer) ||
            !_roster.Contains(GetNetEntity(ev.Target)))
            return;

        switch (ev.NewMobState)
        {
            case MobState.Critical:
                // Валюта и фраг начисляются за уход в критическое состояние, а не за добивание.
                AwardKill(ev.Origin);
                break;

            case MobState.Dead:
                RecordDeath(ev.Target);
                OnArenaDeath(ev.Target, arenaPlayer);
                break;
        }
    }

    private void OnArenaDeath(EntityUid victim, ArenaPlayerComponent arenaPlayer)
    {
        var victimNet = GetNetEntity(victim);

        _playerTeams.Remove(victimNet);

        // Игрок сам вышел в гост из критического состояния (команда/verb ghost):
        // тело не возрождаем, игрок остаётся призраком. Mind уже уйдёт в гост сам.
        if (_ghostOutRequests.Remove(victimNet))
        {
            _roster.Remove(victimNet);
            QueueDel(victim);
            return;
        }

        // TDM: мгновенное возрождение с сохранённым пресетом.
        if (CurrentMode == ArenaMode.TDM &&
            arenaPlayer.SavedPresetIndex >= 0 &&
            arenaPlayer.SavedPresetIndex < _presets.Count &&
            RespawnWithSavedPreset(victim, arenaPlayer))
        {
            return;
        }

        RestorePlayer(victim, arenaPlayer);
    }

    /// <summary>
    /// Помечает тело участника TDM, который вышел в гост из критического состояния.
    /// Последующая гибель (крит-килл командой ghost) не должна создавать новое тело.
    /// </summary>
    private void OnGhostAttempt(GhostAttemptHandleEvent ev)
    {
        if (CurrentMode != ArenaMode.TDM || !_cfg.GetCVar(CCVars.GhostKillCrit))
            return;

        if (ev.Mind.CurrentEntity is not { Valid: true } body)
            return;

        if (!_roster.Contains(GetNetEntity(body)))
            return;

        if (!TryComp<MobStateComponent>(body, out var mobState) ||
            mobState.CurrentState != MobState.Critical)
            return;

        _ghostOutRequests.Add(GetNetEntity(body));
    }

    /// <summary>
    /// Учитывает смерть участника арены в статистике раунда.
    /// </summary>
    private void RecordDeath(EntityUid victim)
    {
        if (!_minds.TryGetMind(victim, out _, out var mind) || mind.UserId is not { } userId)
            return;

        var record = GetRecord(userId);
        record.Deaths++;
        if (string.IsNullOrEmpty(record.PlayerName) &&
            _player.TryGetSessionById(userId, out var session))
        {
            record.PlayerName = session.Name;
        }

        if (CurrentMode == ArenaMode.TDM)
        {
            _persistTdmDeaths.TryAdd(userId, 0);
            _persistTdmDeaths[userId]++;
        }
        else if (CurrentMode == ArenaMode.Deathmatch)
        {
            _dmDeaths.TryAdd(userId, 0);
            _dmDeaths[userId]++;
            _persistDmDeaths.TryAdd(userId, 0);
            _persistDmDeaths[userId]++;
        }
        CachePlayerName(userId);
    }

    /// <summary>
    /// Начисляет валюту и фраг игроку, который вывел участника арены в критическое состояние.
    /// </summary>
    private void AwardKill(EntityUid? attacker)
    {
        if (attacker is not { Valid: true })
            return;

        // Ищем владельца урона по цепочке родителей (пули, осколки и т.п.).
        if (!TryGetKillerMind(attacker, out var killerMind, out _) || killerMind == null)
            return;

        var killerEnt = killerMind.OwnedEntity;
        if (killerEnt is not { } killerUid || !_roster.Contains(GetNetEntity(killerUid)))
            return;

        if (killerMind.UserId is not { } userId)
            return;

        var record = GetRecord(userId);
        record.Kills++;
        if (string.IsNullOrEmpty(record.PlayerName) &&
            _player.TryGetSessionById(userId, out var session))
        {
            record.PlayerName = session.Name;
        }

        _killCurrency.TryGetValue(userId, out var current);
        _killCurrency[userId] = current + ArenaConstants.KillCurrencyReward;

        // Статистика режима: в TDM фраг идёт команде, в Deathmatch — лично.
        if (CurrentMode == ArenaMode.TDM &&
            TryComp<ArenaPlayerComponent>(killerUid, out var killerArena) &&
            killerArena.Team != ArenaTeam.None)
        {
            _tdmTeamKills[killerArena.Team]++;
            _persistTdmKills.TryAdd(userId, 0);
            _persistTdmKills[userId]++;
            if (killerArena.Team == ArenaTeam.Blue)
                _persistTdmBlueKills++;
            else
                _persistTdmRedKills++;
        }
        else if (CurrentMode == ArenaMode.Deathmatch)
        {
            _dmKills.TryAdd(userId, 0);
            _dmKills[userId]++;
            _persistDmKills.TryAdd(userId, 0);
            _persistDmKills[userId]++;
        }
        CachePlayerName(userId);

        // Подлечиваем убийцу — награда за агрессию, а не за победу в перестрелке.
        _damageable.SetAllDamage(killerUid, FixedPoint2.Zero);
    }

    private void CachePlayerName(NetUserId userId)
    {
        try
        {
            if (_player.TryGetSessionById(userId, out var session))
                _persistPlayerNames[userId] = session.Name;
            else
                _persistPlayerNames[userId] = "Unknown";
        }
        catch
        {
            _persistPlayerNames[userId] = "Unknown";
        }
    }

    private ArenaPlayerRecord GetRecord(NetUserId userId)
    {
        if (!_records.TryGetValue(userId, out var record))
        {
            record = new ArenaPlayerRecord();
            _records[userId] = record;
        }

        return record;
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        SendManifest();
    }

    /// <summary>
    /// Собирает итоги арены за раунд: участники за раунд (K/D) и накопленная статистика режимов.
    /// </summary>
    private void SendManifest()
    {
        if (_records.Count == 0 && _persistDmKills.Count == 0 && _persistTdmKills.Count == 0)
            return;

        var records = new List<ArenaPlayerRecord>();
        foreach (var (userId, record) in _records)
        {
            // Дозаполняем имена на момент отправки — игрок может успеть отключиться.
            if (string.IsNullOrEmpty(record.PlayerName) &&
                _player.TryGetSessionById(userId, out var session))
            {
                record.PlayerName = session.Name;
            }

            record.KD = record.Deaths > 0 ? (double)record.Kills / record.Deaths : record.Kills;
            records.Add(record);
        }

        records.Sort((a, b) =>
        {
            var byKd = b.KD.CompareTo(a.KD);
            return byKd != 0 ? byKd : b.Kills.CompareTo(a.Kills);
        });

        var dmPlayers = BuildModeRecords(_persistDmKills, _persistDmDeaths);
        var tdmPlayers = BuildModeRecords(_persistTdmKills, _persistTdmDeaths);

        ArenaTeam? bestTdmTeam = _persistTdmBlueKills > _persistTdmRedKills ? ArenaTeam.Blue :
                                 _persistTdmRedKills > _persistTdmBlueKills ? ArenaTeam.Red : null;

        ArenaPlayerRecord? overallBest = null;
        var allUserIds = new HashSet<NetUserId>(_persistDmKills.Keys);
        allUserIds.UnionWith(_persistTdmKills.Keys);
        foreach (var userId in allUserIds)
        {
            var dmK = _persistDmKills.GetValueOrDefault(userId, 0);
            var dmD = _persistDmDeaths.GetValueOrDefault(userId, 0);
            var tdmK = _persistTdmKills.GetValueOrDefault(userId, 0);
            var tdmD = _persistTdmDeaths.GetValueOrDefault(userId, 0);
            var totalK = dmK + tdmK;
            var totalD = dmD + tdmD;
            var kd = totalD == 0 ? totalK : (double)totalK / totalD;
            if (overallBest == null || kd > overallBest.KD)
            {
                overallBest = new ArenaPlayerRecord
                {
                    PlayerName = _persistPlayerNames.GetValueOrDefault(userId, "Unknown"),
                    Kills = totalK,
                    Deaths = totalD,
                    KD = kd,
                    DmKills = dmK,
                    DmDeaths = dmD,
                    TdmKills = tdmK,
                    TdmDeaths = tdmD,
                };
            }
        }

        RaiseNetworkEvent(new ArenaManifestEvent
        {
            Players = records,
            DmPlayers = dmPlayers,
            TdmPlayers = tdmPlayers,
            BestTdmTeam = bestTdmTeam,
            BlueScore = _persistTdmBlueKills,
            RedScore = _persistTdmRedKills,
            OverallBest = overallBest,
        });
    }

    private List<ArenaPlayerRecord> BuildModeRecords(Dictionary<NetUserId, int> kills, Dictionary<NetUserId, int> deaths)
    {
        var list = new List<ArenaPlayerRecord>();
        foreach (var (userId, killCount) in kills)
        {
            var deathCount = deaths.GetValueOrDefault(userId, 0);
            var record = new ArenaPlayerRecord
            {
                PlayerName = _persistPlayerNames.GetValueOrDefault(userId, "Unknown"),
                Kills = killCount,
                Deaths = deathCount,
                KD = deathCount == 0 ? killCount : (double)killCount / deathCount,
            };
            list.Add(record);
        }

        return list.OrderByDescending(p => p.KD).Take(10).ToList();
    }

    // ============================================================
    // Игровые режимы
    // ============================================================

    private void StartDeathmatch()
    {
        _playerTeams.Clear();
        _dmKills.Clear();
        _dmDeaths.Clear();
        _tdmTeamKills[ArenaTeam.Blue] = 0;
        _tdmTeamKills[ArenaTeam.Red] = 0;
        RoundState = ArenaRoundState.Active;
        RoundTimeRemaining = DeathmatchDuration;
        CurrentMode = ArenaMode.Deathmatch;
        RoundStarted = true;
        foreach (var netEnt in _roster)
        {
            if (!TryGetEntity(netEnt, out var uid))
                continue;
            RemComp<PacifiedComponent>(uid.Value);
        }
        BroadcastRoundState();
        Log.Info("Arena deathmatch started");
    }

    private void StartTDM()
    {
        _playerTeams.Clear();
        _dmKills.Clear();
        _dmDeaths.Clear();
        _tdmTeamKills[ArenaTeam.Blue] = 0;
        _tdmTeamKills[ArenaTeam.Red] = 0;
        CurrentMode = ArenaMode.TDM;
        RoundStarted = true;
        CacheTeamSpawns();
        CacheTDMDoors();
        AssignTDTeams();
        RespawnAllForTDM();
        CloseTDMDoors();
        _cleanTick = 0f;
        RoundState = ArenaRoundState.Preparation;
        RoundTimeRemaining = TDMPreparationDuration;
        BroadcastRoundState();
        Log.Info("Arena TDM — preparation phase started");
    }

    private void CacheTeamSpawns()
    {
        _blueSpawns.Clear();
        _redSpawns.Clear();
        if (_arenaMap is not { } map)
            return;

        var cursor = AllEntityQuery<ArenaTeamSpawnComponent, TransformComponent>();
        while (cursor.MoveNext(out _, out var teamSpawn, out var xform))
        {
            if (xform.MapID != Transform(map).MapID)
                continue;
            if (teamSpawn.Team == ArenaTeam.Blue)
                _blueSpawns.Add(xform.Coordinates);
            else if (teamSpawn.Team == ArenaTeam.Red)
                _redSpawns.Add(xform.Coordinates);
        }
    }

    private void CacheTDMDoors()
    {
        _tdmDoors.Clear();
        if (_arenaMap is not { } map)
            return;

        var mid = Transform(map).MapID;
        var cursor = AllEntityQuery<DoorComponent, TransformComponent>();
        while (cursor.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.MapID == mid)
                _tdmDoors.Add(uid);
        }
    }

    private void CloseTDMDoors()
    {
        foreach (var door in _tdmDoors)
        {
            if (TryComp<DoorComponent>(door, out var doorComp) && doorComp.State == DoorState.Open)
                _doorSystem.StartClosing(door);
        }
    }

    private void OpenTDMDoors()
    {
        foreach (var door in _tdmDoors)
        {
            if (TryComp<DoorComponent>(door, out var doorComp) && doorComp.State != DoorState.Open)
                _doorSystem.StartOpening(door);
        }
    }

    private void AssignTDTeams()
    {
        _tdmTeamLocks.Clear();
        var players = _roster.ToList();
        _luck.Shuffle(players);
        var half = players.Count / 2;
        for (var i = 0; i < players.Count; i++)
        {
            var team = i < half ? ArenaTeam.Blue : ArenaTeam.Red;
            _playerTeams[players[i]] = team;
            // Lock player to their team for this TDM round
            if (TryGetEntity(players[i], out var uid) &&
                _minds.TryGetMind(uid.Value, out _, out var mind) &&
                mind?.UserId is { } userId)
            {
                _tdmTeamLocks[userId] = team;
            }
        }
        Log.Info($"TDM: {_playerTeams.Count(v => v.Value == ArenaTeam.Blue)} blue, {_playerTeams.Count(v => v.Value == ArenaTeam.Red)} red");
    }

    private void RespawnAllForTDM()
    {
        var oldRoster = _roster.ToList();
        _roster.Clear();
        // Delete old bodies and spawn new ones with team equipment
        foreach (var netEnt in oldRoster)
        {
            if (!TryGetEntity(netEnt, out var oldUid))
                continue;
            if (!TryComp<ArenaPlayerComponent>(oldUid, out var arenaPlayer))
                continue;
            if (!_minds.TryGetMind(oldUid.Value, out var mindId, out var mind))
                continue;

            var team = _playerTeams.GetValueOrDefault(netEnt, ArenaTeam.Blue);
            // Use team default preset (saved preset is only for mid-round auto-respawn)
            var nullablePreset = _presets.FirstOrDefault(p => p.Team == team && p.Mode == ArenaMode.TDM);
            if (nullablePreset == null)
                nullablePreset = _presets.FirstOrDefault();
            if (nullablePreset == null)
                continue;
            var preset = nullablePreset;

            // Spawn at team spawn
            var spot = GetTeamSpawn(team);
            string speciesId;
            HumanoidCharacterProfile? profile = null;
            if (mind.UserId != null)
            {
                profile = _prefs.GetPreferences(mind.UserId.Value).SelectedCharacter as HumanoidCharacterProfile;
                speciesId = profile?.Species ?? SharedHumanoidAppearanceSystem.DefaultSpecies;
            }
            else
            {
                speciesId = SharedHumanoidAppearanceSystem.DefaultSpecies;
            }

            // Force human species for Vox/IPC
            if (profile != null && ArenaConstants.SpeciesBlacklist.Contains(speciesId))
            {
                profile = profile.WithSpecies(SharedHumanoidAppearanceSystem.DefaultSpecies)
                    .WithCharacterAppearance(HumanoidCharacterAppearance.DefaultWithSpecies(SharedHumanoidAppearanceSystem.DefaultSpecies));
                speciesId = SharedHumanoidAppearanceSystem.DefaultSpecies;
            }

            var species = _protos.Index<SpeciesPrototype>(speciesId);
            var fresh = Spawn(species.Prototype, spot);
            var entityName = mind.CharacterName ?? "Unknown";
            _meta.SetEntityName(fresh, entityName);
            if (profile != null)
                _humanoid.LoadProfile(fresh, profile);

            _stationSpawning.EquipStartingGear(fresh, preset, raiseEvent: false);
            if (mind.UserId is { } tdmUserId)
            {
                ApplyTdmPurchases(fresh, tdmUserId);
                EquipCostumes(fresh, tdmUserId);
            }
            ApplyTdmTeamClothing(fresh, team);

            var newArenaPlayer = EnsureComp<ArenaPlayerComponent>(fresh);
            newArenaPlayer.OriginalMind = arenaPlayer.OriginalMind;
            newArenaPlayer.OriginalGhost = arenaPlayer.OriginalGhost;
            newArenaPlayer.CanReturnToBody = arenaPlayer.CanReturnToBody;
            newArenaPlayer.Team = team;
            newArenaPlayer.SavedPresetIndex = _presets.IndexOf(preset);
            EnsureComp<AntagImmuneComponent>(fresh);
            EnsureComp<PacifiedComponent>(fresh);
            _minds.TransferTo(mindId, fresh, mind: mind);

            // Delete old body
            QueueDel(oldUid.Value);
            var newNetEnt = GetNetEntity(fresh);
            _roster.Add(newNetEnt);
            _playerTeams[newNetEnt] = team;
        }
    }

    private EntityCoordinates GetTeamSpawn(ArenaTeam team)
    {
        var spawns = team == ArenaTeam.Blue ? _blueSpawns : _redSpawns;
        if (spawns.Count > 0)
            return _luck.Pick(spawns);
        if (_arenaMap is { } map)
            return new EntityCoordinates(map, System.Numerics.Vector2.Zero);
        return EntityCoordinates.Invalid;
    }

    private void StartTDMActive()
    {
        RoundState = ArenaRoundState.Active;
        RoundTimeRemaining = TDMRoundDuration;
        foreach (var netEnt in _roster)
        {
            if (!TryGetEntity(netEnt, out var uid))
                continue;
            RemComp<PacifiedComponent>(uid.Value);
        }
        OpenTDMDoors();
        BroadcastRoundState();
        Log.Info("Arena TDM — round started");
    }

    private void StartIntermission()
    {
        _playerTeams.Clear();
        _votes.Clear();
        RoundState = ArenaRoundState.Intermission;
        RoundTimeRemaining = IntermissionDuration;
        RoundStarted = true;
        foreach (var netEnt in _roster)
        {
            if (!TryGetEntity(netEnt, out var uid))
                continue;
            EnsureComp<PacifiedComponent>(uid.Value);
        }
        BroadcastRoundState();
        BroadcastVoteState();
        Log.Info("Arena intermission started");
    }

    private void BroadcastRoundState()
    {
        var ev = new ArenaRoundUpdateEvent(CurrentMode, RoundState, RoundTimeRemaining,
            _tdmTeamKills[ArenaTeam.Blue], _tdmTeamKills[ArenaTeam.Red]);
        RaiseNetworkEvent(ev, Filter.Broadcast());
    }

    private void BroadcastVoteState()
    {
        var available = new List<ArenaMode> { ArenaMode.Deathmatch, ArenaMode.TDM };
        var ev = new ArenaVoteStateEvent(available, new Dictionary<NetEntity, ArenaMode>(_votes));
        RaiseNetworkEvent(ev, Filter.Broadcast());
    }

    private void BroadcastRoundEndWinner()
    {
        switch (CurrentMode)
        {
            case ArenaMode.Deathmatch:
            {
                NetUserId? bestPlayer = null;
                var bestKd = -1.0;
                foreach (var (userId, kills) in _dmKills)
                {
                    var deaths = _dmDeaths.GetValueOrDefault(userId, 0);
                    var kd = deaths == 0 ? kills : (double)kills / deaths;
                    if (kd > bestKd)
                    {
                        bestKd = kd;
                        bestPlayer = userId;
                    }
                }
                if (bestPlayer is { } winner)
                {
                    var name = _prefs.GetPreferences(winner).SelectedCharacter?.Name ?? "Unknown";
                    var kills = _dmKills.GetValueOrDefault(winner, 0);
                    var deaths = _dmDeaths.GetValueOrDefault(winner, 0);
                    _chat.ChatMessageToAll(ChatChannel.Server,
                        Loc.GetString("arena-winner-dm", ("name", name), ("kills", kills), ("deaths", deaths)),
                        Loc.GetString("arena-winner-dm-wrap", ("name", name), ("kills", kills), ("deaths", deaths)),
                        EntityUid.Invalid, false, true, Color.OrangeRed);
                }
                break;
            }
            case ArenaMode.TDM:
            {
                ArenaTeam winner;
                if (_tdmTeamKills[ArenaTeam.Blue] > _tdmTeamKills[ArenaTeam.Red])
                    winner = ArenaTeam.Blue;
                else if (_tdmTeamKills[ArenaTeam.Red] > _tdmTeamKills[ArenaTeam.Blue])
                    winner = ArenaTeam.Red;
                else
                {
                    // Draw — no winner
                    _chat.ChatMessageToAll(ChatChannel.Server,
                        Loc.GetString("arena-winner-tdm-draw"),
                        Loc.GetString("arena-winner-tdm-draw-wrap"),
                        EntityUid.Invalid, false, true, Color.OrangeRed);
                    break;
                }
                var teamName = winner == ArenaTeam.Blue
                    ? Loc.GetString("arena-tdm-team-blue")
                    : Loc.GetString("arena-tdm-team-red");
                _chat.ChatMessageToAll(ChatChannel.Server,
                    Loc.GetString("arena-winner-tdm", ("team", teamName)),
                    Loc.GetString("arena-winner-tdm-wrap", ("team", teamName)),
                    EntityUid.Invalid, false, true, Color.OrangeRed);
                break;
            }
        }
    }

    private void TallyVotes()
    {
        var dmVotes = _votes.Values.Count(v => v == ArenaMode.Deathmatch);
        var tdmVotes = _votes.Values.Count(v => v == ArenaMode.TDM);
        if (tdmVotes > dmVotes)
            NextMode = ArenaMode.TDM;
        else
            NextMode = ArenaMode.Deathmatch;
        _votes.Clear();
    }

    private void OnVoteCast(ArenaVoteCastEvent msg, EntitySessionEventArgs args)
    {
        var who = (ICommonSession)args.SenderSession;
        if (RoundState != ArenaRoundState.Intermission)
            return;
        if (who.AttachedEntity is not { Valid: true } uid ||
            !TryComp<ArenaPlayerComponent>(uid, out _) ||
            !_roster.Contains(GetNetEntity(uid)))
            return;
        var netEnt = GetNetEntity(uid);
        _votes[netEnt] = msg.Vote;
        BroadcastVoteState();
    }

    private void TickRound(float frameTime)
    {
        if (!RoundStarted || !Enabled || _arenaMap == null)
            return;

        RoundTimeRemaining -= frameTime;
        _broadcastTimer -= frameTime;
        if (_broadcastTimer <= 0f)
        {
            _broadcastTimer = 1f;
            BroadcastRoundState();
        }

        if (RoundTimeRemaining > 0f)
            return;

        switch (RoundState)
        {
            case ArenaRoundState.Intermission:
                TallyVotes();
                CurrentMode = NextMode;
                if (CurrentMode == ArenaMode.TDM)
                    StartTDM();
                else
                    StartDeathmatch();
                break;
            case ArenaRoundState.Preparation:
                StartTDMActive();
                break;
            case ArenaRoundState.Active:
                BroadcastRoundEndWinner();
                // Clean up dead bodies before intermission
                SweepArenaBodies();
                ZapArena();
                StartIntermission();
                break;
        }
    }

    /// <summary>
    /// Завершает текущий раунд (например, админ-командой).
    /// </summary>
    public void EndRound()
    {
        if (RoundState != ArenaRoundState.Active)
            return;
        StartIntermission();
    }

    private bool TryGetKillerMind(EntityUid? origin, out MindComponent? mind, out EntityUid mindId)
    {
        mind = null;
        mindId = default;
        if (origin == null)
            return false;
        if (_minds.TryGetMind(origin.Value, out mindId, out mind) && mind != null)
            return true;
        // Try parent chain (projectiles, vehicles, etc.)
        if (TryComp(origin.Value, out TransformComponent? xform))
        {
            var current = origin.Value;
            for (var i = 0; i < 5; i++)
            {
                var parent = xform.ParentUid;
                if (!parent.IsValid() || parent == current)
                    break;
                if (_minds.TryGetMind(parent, out mindId, out mind) && mind != null)
                    return true;
                current = parent;
            }
        }
        return false;
    }

    // ============================================================
    // Покупки и магазин TDM
    // ============================================================

    private static readonly ProtoId<ArenaStorePrototype> ArenaStoreId = new("ArenaStore");

    /// <summary>
    /// ID листингов аплинка, запрещённых в магазине арены (задаётся в прототипе arenaStore).
    /// </summary>
    private HashSet<string> GetArenaStoreExcludedListings()
    {
        if (!_protos.TryIndex(ArenaStoreId, out var store))
            return new HashSet<string>();

        var result = new HashSet<string>();
        foreach (var excluded in store.ExcludedListings)
            result.Add(excluded.Id);
        return result;
    }

    private List<ArenaTdmListingData> GetTdmStoreListings()
    {
        var uplinkCategories = new HashSet<ProtoId<StoreCategoryPrototype>>
        {
            "UplinkWeaponry", "UplinkAmmo", "UplinkExplosives", "UplinkChemicals",
            "UplinkDeception", "UplinkDisruption", "UplinkImplants", "UplinkAllies",
            "UplinkWearables", "UplinkJob", "UplinkPointless", "UplinkPresets",
        };
        var excluded = GetArenaStoreExcludedListings();
        var result = new List<ArenaTdmListingData>();
        foreach (var listing in _protos.EnumeratePrototypes<ListingPrototype>())
        {
            if (excluded.Contains(listing.ID))
                continue;

            if (!listing.Categories.Overlaps(uplinkCategories))
                continue;
            var cost = listing.Cost.GetValueOrDefault("Telecrystal", FixedPoint2.Zero);
            if (cost == 0)
                continue;
            result.Add(new ArenaTdmListingData
            {
                Id = listing.ID,
                Name = listing.Name ?? string.Empty,
                Description = listing.Description ?? string.Empty,
                Cost = (int)cost.Int(),
                SpritePrototype = listing.ProductEntity ?? string.Empty,
                Category = listing.Categories.Count > 0 ? listing.Categories.First().Id : string.Empty,
            });
        }
        return result;
    }

    public void SetTdmPurchases(NetUserId userId, List<string> listingIds)
    {
        // Validate that all IDs are real store listings
        var validIds = new HashSet<string>(GetTdmStoreListings().Select(l => l.Id));
        var filtered = listingIds.Where(id => validIds.Contains(id)).ToList();
        if (filtered.Count > 0)
            _tdmPurchases[userId] = filtered;
        else
            _tdmPurchases.Remove(userId);
    }

    private void ApplyTdmPurchases(EntityUid fresh, NetUserId userId)
    {
        if (!_tdmPurchases.TryGetValue(userId, out var listingIds) || listingIds.Count == 0)
            return;
        if (!_inventory.TryGetSlotEntity(fresh, "back", out var backpack) ||
            !TryComp<StorageComponent>(backpack, out var storage))
            return;

        // Collect proto IDs already on the character for dedup
        var existingItems = new HashSet<string>();
        if (TryComp<InventoryComponent>(fresh, out var inv))
        {
            var checkEnumerator = _inventory.GetSlotEnumerator((fresh, inv));
            while (checkEnumerator.NextItem(out var equipItem, out _))
            {
                var protoId = MetaData(equipItem).EntityPrototype?.ID;
                if (!string.IsNullOrEmpty(protoId))
                    existingItems.Add(protoId);
                if (TryComp<StorageComponent>(equipItem, out var eqStorage))
                {
                    foreach (var stored in eqStorage.Container.ContainedEntities)
                    {
                        var storedProto = MetaData(stored).EntityPrototype?.ID;
                        if (!string.IsNullOrEmpty(storedProto))
                            existingItems.Add(storedProto);
                    }
                }
            }
        }
        if (TryComp<HandsComponent>(fresh, out var hands))
        {
            foreach (var hand in _hands.EnumerateHands((fresh, hands)))
            {
                if (!_hands.TryGetHeldItem((fresh, hands), hand, out var held))
                    continue;
                var protoId = MetaData(held.Value).EntityPrototype?.ID;
                if (!string.IsNullOrEmpty(protoId))
                    existingItems.Add(protoId);
            }
        }

        var coords = Transform(fresh).Coordinates;
        foreach (var listingId in listingIds)
        {
            var listing = _protos.Index<ListingPrototype>(listingId);
            if (listing.ProductEntity is not { } product)
                continue;
            if (existingItems.Contains(product))
                continue;
            var item = Spawn(product, coords);
            _storage.Insert(backpack.Value, item, out _, storageComp: storage, playSound: false);
            existingItems.Add(product);
        }
    }

    private bool RespawnWithSavedPreset(EntityUid oldBody, ArenaPlayerComponent arenaPlayer)
    {
        if (!_minds.TryGetMind(oldBody, out var mindId, out var mind))
        {
            QueueDel(oldBody);
            return false;
        }

        var preset = _presets[arenaPlayer.SavedPresetIndex];
        var team = preset.Team;
        if (team == ArenaTeam.None)
            team = arenaPlayer.Team;
        // Safety: if team lock exists and preset doesn't match, ghost instead of spawning on wrong team
        if (mind.UserId is { } lockUserId &&
            _tdmTeamLocks.TryGetValue(lockUserId, out var lockedTeam) &&
            lockedTeam != team)
        {
            return false;
        }

        var spot = GetTeamSpawn(team);
        var speciesId = SharedHumanoidAppearanceSystem.DefaultSpecies;
        HumanoidCharacterProfile? profile = null;
        if (mind.UserId != null)
        {
            profile = _prefs.GetPreferences(mind.UserId.Value).SelectedCharacter as HumanoidCharacterProfile;
            if (profile != null)
                speciesId = profile.Species;
        }

        var species = _protos.Index<SpeciesPrototype>(speciesId);
        var fresh = Spawn(species.Prototype, spot);
        if (profile != null)
            _humanoid.LoadProfile(fresh, profile);
        _meta.SetEntityName(fresh, mind.CharacterName ?? "Unknown");
        _stationSpawning.EquipStartingGear(fresh, preset, raiseEvent: false);
        if (mind.UserId is { } userId)
        {
            ApplyTdmPurchases(fresh, userId);
            EquipCostumes(fresh, userId);
        }

        if (team != ArenaTeam.None)
            ApplyTdmTeamClothing(fresh, team);

        var newArenaPlayer = EnsureComp<ArenaPlayerComponent>(fresh);
        newArenaPlayer.OriginalMind = arenaPlayer.OriginalMind;
        newArenaPlayer.OriginalGhost = arenaPlayer.OriginalGhost;
        newArenaPlayer.CanReturnToBody = arenaPlayer.CanReturnToBody;
        newArenaPlayer.Team = team;
        newArenaPlayer.SavedPresetIndex = arenaPlayer.SavedPresetIndex;
        EnsureComp<AntagImmuneComponent>(fresh);
        if (RoundState == ArenaRoundState.Intermission)
            EnsureComp<PacifiedComponent>(fresh);

        _minds.TransferTo(mindId, fresh, mind: mind);
        QueueDel(oldBody);
        _roster.Remove(GetNetEntity(oldBody));
        _playerTeams.Remove(GetNetEntity(oldBody));
        var newNetEnt = GetNetEntity(fresh);
        _roster.Add(newNetEnt);
        _playerTeams[newNetEnt] = team;
        return true;
    }

    // ============================================================
    // Костюмы и валюта
    // ============================================================

    /// <summary>
    /// Покупка костюма за валюту убийств.
    /// </summary>
    public bool TryBuyCostume(ICommonSession session, int costumeIndex)
    {
        if (costumeIndex < 0 || costumeIndex >= _costumes.Count)
            return false;

        var costume = _costumes[costumeIndex];

        var owned = GetOwned(session.UserId);
        if (owned.Contains(costume.ID))
            return false;

        _killCurrency.TryGetValue(session.UserId, out var balance);
        if (balance < costume.Price)
            return false;

        _killCurrency[session.UserId] = balance - costume.Price;
        owned.Add(costume.ID);
        return true;
    }

    /// <summary>
    /// Сохраняет выбранный набор надетой одежды для игрока.
    /// </summary>
    public void SetEquippedCostumes(ICommonSession session, List<int> costumeIndexes)
    {
        var owned = GetOwned(session.UserId);
        var equipped = GetEquipped(session.UserId);

        equipped.Clear();
        foreach (var index in costumeIndexes)
        {
            if (index < 0 || index >= _costumes.Count)
                continue;

            var costume = _costumes[index];
            if (owned.Contains(costume.ID))
                equipped.Add(costume.ID);
        }
    }

    private HashSet<string> GetOwned(NetUserId userId)
    {
        if (!_ownedCostumes.TryGetValue(userId, out var owned))
        {
            owned = new HashSet<string>();
            _ownedCostumes[userId] = owned;
        }

        return owned;
    }

    private List<string> GetEquipped(NetUserId userId)
    {
        if (!_equippedCostumes.TryGetValue(userId, out var equipped))
        {
            equipped = new List<string>();
            _equippedCostumes[userId] = equipped;
        }

        return equipped;
    }

    // ============================================================
    // Лоадаут и спавн
    // ============================================================

    public ArenaLoadoutEuiState GetLoadoutState(NetUserId userId)
    {
        if (_presets.Count == 0)
            RefreshPresets();

        var options = new List<ArenaLoadoutOption>();
        for (var i = 0; i < _presets.Count; i++)
        {
            var p = _presets[i];
            // Пресеты режима показываются только в соответствующем режиме.
            if (CurrentMode == ArenaMode.TDM && p.Mode != ArenaMode.TDM)
                continue;
            if (CurrentMode != ArenaMode.TDM && p.Mode == ArenaMode.TDM)
                continue;
            options.Add(new ArenaLoadoutOption
            {
                Index = i,
                Name = p.NameLoc,
                Description = p.DescLoc,
                Category = p.Category,
                SpritePrototype = p.IconPrototype,
            });
        }

        var costumes = new List<ArenaCostumeOption>();
        for (var i = 0; i < _costumes.Count; i++)
        {
            var c = _costumes[i];
            costumes.Add(new ArenaCostumeOption
            {
                Index = i,
                Id = c.ID,
                Name = c.NameLoc,
                Description = c.DescLoc,
                Category = c.Category,
                ItemPrototype = c.Item,
                Slot = c.Slot,
                Price = c.Price,
            });
        }

        _killCurrency.TryGetValue(userId, out var balance);

        var owned = GetOwned(userId);
        var ownedIndexes = new HashSet<int>();
        for (var i = 0; i < _costumes.Count; i++)
        {
            if (owned.Contains(_costumes[i].ID))
                ownedIndexes.Add(i);
        }

        var equipped = GetEquipped(userId);
        var equippedIndexes = new List<int>();
        for (var i = 0; i < _costumes.Count; i++)
        {
            if (equipped.Contains(_costumes[i].ID))
                equippedIndexes.Add(i);
        }

        var storeListings = GetTdmStoreListings();
        var purchased = _tdmPurchases.TryGetValue(userId, out var list) ? list : new List<string>();
        var spent = storeListings.Where(l => purchased.Contains(l.Id)).Sum(l => l.Cost);
        var remaining = TdmStoreBudget - spent;

        return new ArenaLoadoutEuiState(options, costumes, balance, ownedIndexes, equippedIndexes,
            storeListings, purchased, remaining);
    }

    public bool SpawnPlayer(ArenaLoadoutEui eui, ICommonSession who, EntityUid sourceGhost, int kitIdx)
    {
        if (!CanJoinArena(who))
        {
            if (_prison.IsUserPrisoner(who.UserId))
            {
                _chat.DispatchServerMessage(who, Loc.GetString("prison-arena-blocked"));
                if (!eui.IsShutDown)
                    eui.Close();
            }

            return false;
        }

        if (!_activeEuis.TryGetValue(who, out var currentEui) ||
            !ReferenceEquals(currentEui, eui) ||
            who.AttachedEntity != sourceGhost ||
            !TryComp<GhostComponent>(sourceGhost, out var ghost))
            return false;

        if (!_minds.TryGetMind(who, out var originalMindId, out var originalMind))
            return false;

        EnsureMap();

        if (_arenaMap is not { } map)
            return false;

        // Clean up old dead bodies from previous lives
        SweepArenaBodies();

        if (_presets.Count == 0)
            RefreshPresets();

        if (_presets.Count == 0)
            return false;

        var kitIdxClamped = Math.Clamp(kitIdx, 0, _presets.Count - 1);
        var preset = _presets[kitIdxClamped];

        // Позиция спавна зависит от режима и команды.
        EntityCoordinates spot;
        if (CurrentMode == ArenaMode.TDM)
        {
            // Enforce team lock: player must use the same team they were first assigned
            if (_tdmTeamLocks.TryGetValue(who.UserId, out var lockedTeam))
            {
                if (preset.Team != lockedTeam)
                    return false;
            }
            else
            {
                _tdmTeamLocks[who.UserId] = preset.Team;
            }
            spot = GetTeamSpawn(preset.Team);
        }
        else
        {
            var sites = new List<EntityCoordinates>();
            var cursor = AllEntityQuery<ArenaSpawnPointComponent, TransformComponent>();
            while (cursor.MoveNext(out var uid, out _, out var where))
            {
                if (where.MapID != Transform(map).MapID)
                    continue;
                // Don't use TDM team spawns in non-TDM modes
                if (HasComp<ArenaTeamSpawnComponent>(uid))
                    continue;
                sites.Add(where.Coordinates);
            }

            spot = sites.Count > 0
                ? _luck.Pick(sites)
                : new EntityCoordinates(map, System.Numerics.Vector2.Zero);
        }

        var profile = _prefs.GetPreferences(who.UserId).SelectedCharacter as HumanoidCharacterProfile;
        string speciesId = profile?.Species ?? SharedHumanoidAppearanceSystem.DefaultSpecies;

        // Блеклист арены: IPC и Vox на арене всегда спавнятся людьми.
        if (ArenaConstants.SpeciesBlacklist.Contains(speciesId))
        {
            speciesId = SharedHumanoidAppearanceSystem.DefaultSpecies;
            if (profile != null)
                profile = profile.WithSpecies(speciesId);
        }

        var species = _protos.Index<SpeciesPrototype>(speciesId);
        var fresh = Spawn(species.Prototype, spot);

        if (profile != null)
            _humanoid.LoadProfile(fresh, profile);

        _meta.SetEntityName(fresh, who.Name);
        GetRecord(who.UserId).PlayerName = who.Name;

        _stationSpawning.EquipStartingGear(fresh, preset, raiseEvent: false);
        if (CurrentMode == ArenaMode.TDM)
        {
            ApplyTdmPurchases(fresh, who.UserId);
            EquipCostumes(fresh, who.UserId);
            ApplyTdmTeamClothing(fresh, preset.Team);
        }
        else
        {
            EquipCostumes(fresh, who.UserId);
        }

        var arenaPlayer = EnsureComp<ArenaPlayerComponent>(fresh);
        arenaPlayer.OriginalMind = originalMindId;
        arenaPlayer.OriginalGhost = sourceGhost;
        arenaPlayer.CanReturnToBody = ghost.CanReturnToBody;
        arenaPlayer.Team = CurrentMode == ArenaMode.TDM ? preset.Team : ArenaTeam.None;
        arenaPlayer.SavedPresetIndex = kitIdxClamped;
        EnsureComp<AntagImmuneComponent>(fresh);

        if (RoundState == ArenaRoundState.Intermission)
            EnsureComp<PacifiedComponent>(fresh);

        // The disposable arena body must never inherit the round mind's roles or objectives.
        _minds.SetUserId(originalMindId, null, originalMind);
        _minds.TransferTo(originalMindId, null, createGhost: false, mind: originalMind);
        var temporaryMind = _minds.CreateMind(who.UserId, who.Name);
        EnsureComp<ArenaMindComponent>(temporaryMind); // Never include the disposable arena mind in round data.
        _minds.TransferTo(temporaryMind, fresh, mind: temporaryMind.Comp);
        QueueDel(sourceGhost);
        _roles.MindAddJobRole(temporaryMind, silent: true, jobPrototype: "ArenaWarrior");

        var netEnt = GetNetEntity(fresh);
        _roster.Add(netEnt);
        if (CurrentMode == ArenaMode.TDM && arenaPlayer.Team != ArenaTeam.None)
            _playerTeams[netEnt] = arenaPlayer.Team;
        return true;
    }

    private void RestorePlayer(EntityUid body, ArenaPlayerComponent arenaPlayer)
    {
        _roster.Remove(GetNetEntity(body));
        _playerTeams.Remove(GetNetEntity(body));

        if (!_minds.TryGetMind(body, out var temporaryMindId, out var temporaryMind))
        {
            QueueDel(body);
            return;
        }

        var userId = temporaryMind.UserId;

        if (temporaryMind.VisitingEntity != null)
            _minds.UnVisit(temporaryMindId, temporaryMind);

        if (userId == null || !TryComp<MindComponent>(arenaPlayer.OriginalMind, out var originalMind))
        {
            if (userId != null)
                _ghosts.SpawnGhost((temporaryMindId, temporaryMind), body, false);
            else
            {
                _minds.TransferTo(temporaryMindId, null, createGhost: false, mind: temporaryMind);
                QueueDel(temporaryMindId);
            }

            QueueDel(body);
            return;
        }

        _minds.SetUserId(temporaryMindId, null, temporaryMind);
        _minds.TransferTo(temporaryMindId, null, createGhost: false, mind: temporaryMind);

        // The source ghost was queued for deletion when the temporary mind took over.
        if (originalMind.CurrentEntity == arenaPlayer.OriginalGhost)
        {
            if (originalMind.VisitingEntity == arenaPlayer.OriginalGhost)
                _minds.UnVisit(arenaPlayer.OriginalMind, originalMind);
            else if (originalMind.OwnedEntity == arenaPlayer.OriginalGhost)
                _minds.TransferTo(arenaPlayer.OriginalMind, null, createGhost: false, mind: originalMind);
        }

        _minds.SetUserId(arenaPlayer.OriginalMind, userId.Value, originalMind);
        RestoreGhost(body, arenaPlayer, originalMind);

        QueueDel(temporaryMindId);
        QueueDel(body);
    }

    private void RestoreGhost(EntityUid arenaBody, ArenaPlayerComponent arenaPlayer, MindComponent originalMind)
    {
        var canReturn = arenaPlayer.CanReturnToBody &&
            originalMind.OwnedEntity is { } originalBody &&
            Exists(originalBody) &&
            !TerminatingOrDeleted(originalBody) &&
            !HasComp<GhostComponent>(originalBody);

        if (originalMind.CurrentEntity is { } current && TryComp<GhostComponent>(current, out var currentGhost))
        {
            _ghosts.SetCanReturnToBody((current, currentGhost), canReturn);
            return;
        }

        if (canReturn && originalMind.OwnedEntity is { } returnBody)
            _ghosts.SpawnGhost((arenaPlayer.OriginalMind, originalMind), returnBody, true);
        else
            _ghosts.SpawnGhost((arenaPlayer.OriginalMind, originalMind), arenaBody, false);
    }

    /// <summary>
    /// Надевает купленные костюмы на игрока арены, поверх экипировки пресета.
    /// </summary>
    private void EquipCostumes(EntityUid body, NetUserId userId)
    {
        var equipped = GetEquipped(userId);
        if (equipped.Count == 0)
            return;

        foreach (var costumeId in equipped)
        {
            ArenaCostumePrototype? costume = null;
            foreach (var c in _costumes)
            {
                if (c.ID == costumeId)
                {
                    costume = c;
                    break;
                }
            }

            if (costume == null)
                continue;

            if (!_protos.TryIndex<EntityPrototype>(costume.Item, out _))
                continue;

            // Освобождаем слот от штатного снаряжения пресета, чтобы костюм наделся вместо него.
            var item = Spawn(costume.Item, Transform(body).Coordinates);

            // Предметы в слотах, зависящих от заменяемого (карманы комбинезона, suitstorage и т.п.),
            // при снятии старой вещи выпадают на пол. Снимаем их без выброса на землю и
            // вернём в те же слоты после надевания нового костюма.
            var dependent = new List<(string Slot, EntityUid Item)>();
            if (_inventory.TryGetSlotEntity(body, costume.Slot, out var existing))
            {
                if (_inventory.TryGetSlots(body, out var slots))
                {
                    foreach (var slotDef in slots)
                    {
                        if (slotDef.DependsOn != costume.Slot)
                            continue;
                        if (_inventory.TryGetSlotEntity(body, slotDef.Name, out var depItem))
                            dependent.Add((slotDef.Name, depItem.Value));
                    }
                }

                foreach (var (slot, _) in dependent)
                {
                    if (_inventory.TryGetSlotContainer(body, slot, out var depContainer, out _) &&
                        depContainer.ContainedEntity is { } depUid)
                        _container.Remove(depUid, depContainer, reparent: false, force: true);
                }

                // Переносим содержимое карманов старой вещи в новую, чтобы предметы лоадаута не падали на пол.
                MoveStorageContents(existing.Value, item);

                _inventory.TryUnequip(body, costume.Slot, silent: true, force: true);
                QueueDel(existing);
            }

            var equippedOk = _inventory.TryEquip(body, item, costume.Slot, silent: true, force: true);
            if (!equippedOk)
                QueueDel(item);

            // Надеваем сохранённые предметы обратно в зависящие слоты (карманы и т.п.).
            foreach (var (slot, depItem) in dependent)
            {
                if (_inventory.TryGetSlotContainer(body, slot, out var depContainer, out _))
                    _container.Insert(depItem, depContainer, force: true);
            }

            if (!equippedOk)
                continue;

            // На жилеты автоматически применяются резисты уровня ClothingOuterArmorBasic.
            if (costume.Category == "vest")
                ApplyBasicArmor(item);
        }
    }

    /// <summary>
    /// Применяет к предмету резисты базовой брони (ClothingOuterArmorBasic: Blunt/Slash/Piercing/Heat 0.7).
    /// </summary>
    private void ApplyBasicArmor(EntityUid item)
    {
        _armor.SetModifiers(item, new DamageModifierSet
        {
            Coefficients = new Dictionary<string, float>
            {
                ["Blunt"] = 0.7f,
                ["Slash"] = 0.7f,
                ["Piercing"] = 0.7f,
                ["Heat"] = 0.7f,
            },
        }, EnsureComp<ArmorComponent>(item));
    }

    /// <summary>
    /// В режиме TDM жилет, шлем и плащ (слот шеи) игрока становятся неснимаемыми, окрашиваются
    /// в цвет команды, а жилет и шлем получают резисты старых командных прототипов брони (без замедления).
    /// </summary>
    private void ApplyTdmTeamClothing(EntityUid body, ArenaTeam team)
    {
        // Слот бронежилета: резисты старой командной брони (ArenaOuterArmor*: 0.6/0.6/0.6
        // поверх ClothingOuterArmorHeavy с Heat 0.5 и Caustic 0.75), без замедления.
        ApplyTdmSlotClothing(body, "outerClothing", team, new DamageModifierSet
        {
            Coefficients = new Dictionary<string, float>
            {
                ["Blunt"] = 0.6f,
                ["Slash"] = 0.6f,
                ["Piercing"] = 0.6f,
                ["Heat"] = 0.5f,
                ["Caustic"] = 0.75f,
            },
        });

        // Слот шлема: резисты старого командного шлема (ClothingHeadHelmetArmoredBase).
        ApplyTdmSlotClothing(body, "head", team, new DamageModifierSet
        {
            Coefficients = new Dictionary<string, float>
            {
                ["Blunt"] = 0.9f,
                ["Slash"] = 0.9f,
                ["Piercing"] = 0.9f,
                ["Heat"] = 0.9f,
            },
        });

        // Плащ/накидка (слот шеи): только окраска и неснимаемость, без резистов.
        ApplyTdmSlotClothing(body, "neck", team);
    }

    private void ApplyTdmSlotClothing(EntityUid body, string slot, ArenaTeam team, DamageModifierSet? resists = null)
    {
        if (!_inventory.TryGetSlotEntity(body, slot, out var item))
            return;

        var itemUid = item.Value;

        // Одежда становится неснимаемой.
        EnsureComp<SelfUnremovableClothingComponent>(itemUid);

        // Резисты старых командных прототипов брони (для жилета и шлема).
        if (resists != null)
            _armor.SetModifiers(itemUid, resists, EnsureComp<ArmorComponent>(itemUid));

        // Замедление от старой командной брони не применяется.
        RemComp<ClothingSpeedModifierComponent>(itemUid);

        // Метка для клиента: предмет окрашивается в цвет команды.
        var teamClothing = EnsureComp<ArenaTeamClothingComponent>(itemUid);
        teamClothing.Team = team;
        Dirty(itemUid, teamClothing);
    }

    /// <summary>
    /// Переносит содержимое хранилища (карманов) старого предмета в новый, чтобы при замене одежды
    /// предметы из лоадаута не падали на пол.
    /// </summary>
    private void MoveStorageContents(EntityUid oldItem, EntityUid newItem)
    {
        if (!TryComp<StorageComponent>(oldItem, out var oldStorage) ||
            oldStorage.Container == null ||
            !TryComp<StorageComponent>(newItem, out var newStorage))
            return;

        foreach (var content in oldStorage.Container.ContainedEntities.ToArray())
        {
            _storage.Insert(newItem, content, out _, playSound: false, storageComp: newStorage);
        }
    }

    // ============================================================
    // Карта и очистка
    // ============================================================

    private void EnsureMap()
    {
        if (_arenaMap != null && Exists(_arenaMap.Value))
            return;

        if (!RoundStarted)
            StartIntermission();

        var opts = Robust.Shared.EntitySerialization.DeserializationOptions.Default with { InitializeMaps = true };

        if (_loader.TryLoadMap(new ResPath(ArenaMapFile), out var entry, out _, opts))
        {
            _arenaMap = entry.Value.Owner;
            Log.Info($"Arena loaded: {ArenaMapFile}");
            return;
        }

        Log.Info($"No arena map at {ArenaMapFile}, building procedural arena");
        var mapUid = _maps.CreateMap(out _);
        _arenaMap = mapUid;

        var (platform, gridComp) = _mapManager.CreateGridEntity(mapUid);
        var tile = new Tile(_tiles["FloorSteel"].TileId);
        var tileList = new List<(Vector2i, Tile)>();

        for (var x = -8; x <= 8; x++)
        {
            for (var y = -8; y <= 8; y++)
            {
                tileList.Add((new Vector2i(x, y), tile));
            }
        }

        _maps.SetTiles(platform, gridComp, tileList);

        var spawnPositions = new[] { (-3, 0), (3, 0), (0, -3), (0, 3) };

        foreach (var (ox, oy) in spawnPositions)
        {
            var spot = new EntityCoordinates(platform, ox, oy);
            var ent = Spawn(null, spot);
            AddComp<ArenaSpawnPointComponent>(ent);
            _meta.SetEntityName(ent, "Arena Spawn");
        }

        _meta.SetEntityName(mapUid, "Arena");
        _meta.SetEntityName(platform, "Arena Platform");
    }

    private void SweepArenaBodies()
    {
        if (_arenaMap is not { } map || !Exists(map))
            return;

        var mid = Transform(map).MapID;

        var bodyQuery = EntityQueryEnumerator<ArenaPlayerComponent, TransformComponent>();
        while (bodyQuery.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.MapID == mid &&
                !_roster.Contains(GetNetEntity(uid)) &&
                !_minds.TryGetMind(uid, out _, out _))
            {
                QueueDel(uid);
            }
        }

        var ghostQuery = EntityQueryEnumerator<GhostComponent, TransformComponent>();
        while (ghostQuery.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.MapID == mid &&
                !_minds.TryGetMind(uid, out _, out _))
            {
                QueueDel(uid);
            }
        }
    }

    private void ZapArena()
    {
        if (_arenaMap is not { } map || !Exists(map))
            return;

        var mid = Transform(map).MapID;
        var graveyard = new List<EntityUid>();

        var walker = AllEntityQuery<TransformComponent>();
        while (walker.MoveNext(out var thing, out var pose))
        {
            if (!pose.ParentUid.IsValid() || pose.MapID != mid)
                continue;

            if (HasComp<MapGridComponent>(thing))
                continue;

            if (HasComp<ActorComponent>(thing) ||
                _minds.TryGetMind(thing, out _, out _))
            {
                continue;
            }

            if (HasComp<BodyPartComponent>(thing))
                continue;

            if (!HasComp<MapGridComponent>(pose.ParentUid) && pose.ParentUid != map)
                continue;

            if (!pose.Anchored || HasComp<PuddleComponent>(thing))
                graveyard.Add(thing);
        }

        foreach (var cadaver in graveyard)
            QueueDel(cadaver);
    }

    public override void Update(float frameTime)
    {
        TickRound(frameTime);

        _cleanTick += frameTime;
        var threshold = CurrentMode == ArenaMode.TDM ? 180f : 60f;
        if (_cleanTick < threshold)
            return;

        _cleanTick = 0f;

        if (CurrentMode == ArenaMode.TDM && RoundState == ArenaRoundState.Preparation)
            return;

        ZapArena();
    }

    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        if (_activeEuis.TryGetValue(ev.Player, out var eui) && eui.SourceGhost == ev.Entity && !eui.IsShutDown)
            eui.Close();

        if (!TryComp<ArenaPlayerComponent>(ev.Entity, out var arenaPlayer) ||
            !_roster.Contains(GetNetEntity(ev.Entity)))
            return;

        // Player disconnected — full restore to preserve mind state
        if (ev.Player.Status == SessionStatus.Disconnected)
        {
            _playerTeams.Remove(GetNetEntity(ev.Entity));
            RestorePlayer(ev.Entity, arenaPlayer);
            return;
        }

        // Visiting another entity (for example via aghost) is temporary. Keep the arena body for the return.
        if (_minds.TryGetMind(ev.Entity, out _, out var temporaryMind) &&
            temporaryMind.VisitingEntity != null)
        {
            return;
        }

        var netEnt = GetNetEntity(ev.Entity);

        // Player re-attached elsewhere (role change, admin takeover, etc.) — just clean up the arena body
        _roster.Remove(netEnt);
        _playerTeams.Remove(netEnt);
        QueueDel(ev.Entity);
    }

    public void OnLoadoutEuiClosed(ICommonSession session, ArenaLoadoutEui eui)
    {
        if (_activeEuis.TryGetValue(session, out var current) && ReferenceEquals(current, eui))
            _activeEuis.Remove(session);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        var openEuis = new List<ArenaLoadoutEui>(_activeEuis.Values);
        foreach (var eui in openEuis)
        {
            if (!eui.IsShutDown)
                eui.Close();
        }

        var query = EntityQueryEnumerator<ArenaPlayerComponent>();
        while (query.MoveNext(out var uid, out var arenaPlayer))
        {
            if (Exists(arenaPlayer.OriginalMind))
                QueueDel(arenaPlayer.OriginalMind);

            QueueDel(uid);
        }

        _activeEuis.Clear();
        _roster.Clear();
        _playerTeams.Clear();
        _blueSpawns.Clear();
        _redSpawns.Clear();
        _tdmDoors.Clear();
        _arenaMap = null;
        _killCurrency.Clear();
        _ownedCostumes.Clear();
        _equippedCostumes.Clear();
        _records.Clear();
        _tdmTeamLocks.Clear();
        _tdmPurchases.Clear();
        _persistDmKills.Clear();
        _persistDmDeaths.Clear();
        _persistTdmKills.Clear();
        _persistTdmDeaths.Clear();
        _persistPlayerNames.Clear();
        _persistTdmBlueKills = 0;
        _persistTdmRedKills = 0;
        _ghostOutRequests.Clear();
        RoundStarted = false;
        RoundState = ArenaRoundState.Intermission;
        RoundTimeRemaining = IntermissionDuration;
        CurrentMode = ArenaMode.Deathmatch;
    }
}
