using System.Linq;
using Content.Server.Antag.Components;
using Content.Server.Chat.Managers;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.Hands.Systems;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Doors.Systems;
using Content.Server.Stunnable;
using Content.Shared.Body.Part;
using Content.Shared.Chat;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Doors.Components;
using Content.Shared.Fluids.Components;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Storage;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Station;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Network;
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
    [Dependency] private readonly DoorSystem _doorSystem = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;

    private const string ArenaMapFile = "/Maps/_DeadSpace/arena.yml";

    private const float DeathmatchDuration = 600f;
    private const float PropHuntHuntDuration = 300f;
    private const float PropHuntHidingDuration = 30f;
    private const float TDMPreparationDuration = 30f;
    private const float TDMRoundDuration = 600f;
    private const float IntermissionDuration = 25f;

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

    private readonly HashSet<NetEntity> _seekerNetEntities = new();
    private readonly Dictionary<NetEntity, ArenaMode> _votes = new();
    private readonly Dictionary<NetEntity, ArenaTeam> _playerTeams = new();
    private readonly List<EntityCoordinates> _blueSpawns = new();
    private readonly List<EntityCoordinates> _redSpawns = new();
    private readonly List<EntityUid> _tdmDoors = new();
    private readonly Dictionary<NetUserId, int> _dmKills = new();
    private readonly Dictionary<NetUserId, int> _dmDeaths = new();
    private readonly Dictionary<ArenaTeam, int> _tdmTeamKills = new()
    {
        [ArenaTeam.Blue] = 0,
        [ArenaTeam.Red] = 0,
    };
    // Persistent stats across sub-rounds (not cleared between DM/TDM rounds)
    private readonly Dictionary<NetUserId, int> _persistDmKills = new();
    private readonly Dictionary<NetUserId, int> _persistDmDeaths = new();
    private readonly Dictionary<NetUserId, int> _persistTdmKills = new();
    private readonly Dictionary<NetUserId, int> _persistTdmDeaths = new();
    private readonly Dictionary<NetUserId, string> _persistPlayerNames = new();
    private int _persistTdmBlueKills;
    private int _persistTdmRedKills;

    public void EndRound()
    {
        if (RoundState != ArenaRoundState.Active && RoundState != ArenaRoundState.Hiding)
            return;

        StartIntermission();
    }

    private EntityUid? _arenaMap;
    private readonly HashSet<NetEntity> _roster = new();
    private readonly List<ArenaLoadoutPresetPrototype> _presets = new();
    private readonly Dictionary<ICommonSession, ArenaLoadoutEui> _activeEuis = new();
    private float _broadcastTimer;

    public override void Initialize()
    {
        SubscribeNetworkEvent<ArenaJoinEvent>(OnJoin);
        SubscribeNetworkEvent<ArenaLeaveEvent>(OnLeave);
        SubscribeNetworkEvent<ArenaVoteCastEvent>(OnVoteCast);
        SubscribeLocalEvent<MobStateChangedEvent>(OnDeath);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void StartDeathmatch()
    {
        _seekerNetEntities.Clear();
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

    private void StartPropHunt()
    {
        _playerTeams.Clear();
        _dmKills.Clear();
        _dmDeaths.Clear();
        _tdmTeamKills[ArenaTeam.Blue] = 0;
        _tdmTeamKills[ArenaTeam.Red] = 0;
        CurrentMode = ArenaMode.PropHunt;
        RoundStarted = true;

        AssignSeekers();

        RoundState = ArenaRoundState.Hiding;
        RoundTimeRemaining = PropHuntHidingDuration;

        foreach (var netEnt in _roster)
        {
            if (!TryGetEntity(netEnt, out var uid))
                continue;
            RemComp<PacifiedComponent>(uid.Value);
        }

        EquipHiders();
        ApplyHiderPacifism();

        FreezeSeekers();
        NotifySeekers();

        BroadcastRoundState();
        Log.Info("Arena Prop Hunt — hiding phase started");
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
        while (cursor.MoveNext(out var comp, out var xform))
        {
            if (xform.MapID != Transform(map).MapID)
                continue;

            if (comp.Team == ArenaTeam.Blue)
                _blueSpawns.Add(xform.Coordinates);
            else if (comp.Team == ArenaTeam.Red)
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
        var players = _roster.ToList();
        _luck.Shuffle(players);

        var half = players.Count / 2;
        for (var i = 0; i < players.Count; i++)
        {
            var team = i < half ? ArenaTeam.Blue : ArenaTeam.Red;
            _playerTeams[players[i]] = team;
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

            // Find the preset for this team
            var preset = _presets.FirstOrDefault(p => p.Team == team && p.Mode == ArenaMode.TDM);
            if (preset == null)
            {
                preset = _presets.FirstOrDefault();
                if (preset == null)
                    continue;
            }

            // Spawn at team spawn
            var spot = GetTeamSpawn(team);
            string speciesId;
            if (mind.UserId != null)
            {
                var profile = _prefs.GetPreferences(mind.UserId.Value).SelectedCharacter as HumanoidCharacterProfile;
                speciesId = profile?.Species ?? SharedHumanoidAppearanceSystem.DefaultSpecies;
            }
            else
            {
                speciesId = SharedHumanoidAppearanceSystem.DefaultSpecies;
            }

            var species = _protos.Index<SpeciesPrototype>(speciesId);
            var fresh = Spawn(species.Prototype, spot);

            var entityName = mind.CharacterName ?? "Unknown";
            _meta.SetEntityName(fresh, entityName);

            if (mind.UserId != null)
            {
                var profile = _prefs.GetPreferences(mind.UserId.Value).SelectedCharacter as HumanoidCharacterProfile;
                if (profile != null)
                    _humanoid.LoadProfile(fresh, profile);
            }
            _stationSpawning.EquipStartingGear(fresh, preset, raiseEvent: false);

            var newArenaPlayer = EnsureComp<ArenaPlayerComponent>(fresh);
            newArenaPlayer.OriginalMind = arenaPlayer.OriginalMind;
            newArenaPlayer.OriginalGhost = arenaPlayer.OriginalGhost;
            newArenaPlayer.CanReturnToBody = arenaPlayer.CanReturnToBody;
            newArenaPlayer.Team = team;
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

    private void EquipHiders()
    {
        foreach (var netEnt in _roster)
        {
            if (_seekerNetEntities.Contains(netEnt))
                continue;

            if (!TryGetEntity(netEnt, out var uid))
                continue;

            var projector = Spawn("ArenaChameleonProjector", Transform(uid.Value).Coordinates);
            _hands.TryPickupAnyHand(uid.Value, projector);
        }
    }

    private void ApplyHiderPacifism()
    {
        foreach (var netEnt in _roster)
        {
            if (_seekerNetEntities.Contains(netEnt))
                continue;

            if (!TryGetEntity(netEnt, out var uid))
                continue;

            EnsureComp<PacifiedComponent>(uid.Value);
        }
    }

    private void AssignSeekers()
    {
        _seekerNetEntities.Clear();
        var players = _roster.ToList();
        if (players.Count == 0)
            return;

        _luck.Shuffle(players);

        var seekerCount = Math.Max(1, (players.Count + 4) / 6);
        seekerCount = Math.Min(seekerCount, players.Count - 1);

        for (var i = 0; i < seekerCount; i++)
        {
            _seekerNetEntities.Add(players[i]);
            if (TryGetEntity(players[i], out var uid))
                EquipSeeker(uid.Value);
        }

        Log.Info($"Prop Hunt: {_seekerNetEntities.Count} seekers, {players.Count - _seekerNetEntities.Count} hiders");
    }

    private void FreezeSeekers()
    {
        foreach (var netEnt in _seekerNetEntities)
        {
            if (!TryGetEntity(netEnt, out var uid))
                continue;

            _stun.TryAddParalyzeDuration(uid.Value, TimeSpan.FromSeconds(PropHuntHidingDuration));

            var blindfold = Spawn("ClothingEyesBlindfold", Transform(uid.Value).Coordinates);
            if (_inventory.TryEquip(uid.Value, blindfold, "eyes", true, true))
                _seekerBlindfolds[netEnt] = blindfold;
        }
    }

    [DataField]
    public Dictionary<NetEntity, EntityUid> _seekerBlindfolds = new();

    private void NotifySeekers()
    {
        foreach (var netEnt in _seekerNetEntities)
        {
            if (!TryGetEntity(netEnt, out var uid))
                continue;

            if (!TryComp<ActorComponent>(uid.Value, out var actor))
                continue;

            RaiseNetworkEvent(new ArenaSeekerFreezeEvent(PropHuntHidingDuration),
                Filter.SinglePlayer(actor.PlayerSession));
        }
    }

    private void StartPropHuntHunt()
    {
        RoundState = ArenaRoundState.Active;
        RoundTimeRemaining = PropHuntHuntDuration;

        UnfreezeSeekers();

        BroadcastRoundState();
        Log.Info("Arena Prop Hunt — hunt phase started");
    }

    private void UnfreezeSeekers()
    {
        foreach (var netEnt in _seekerNetEntities)
        {
            if (!TryGetEntity(netEnt, out var uid))
                continue;

            _stun.TryUnstun(uid.Value);

            RemComp<PacifiedComponent>(uid.Value);

            if (_seekerBlindfolds.TryGetValue(netEnt, out var blindfold))
            {
                _inventory.TryUnequip(uid.Value, "eyes");
                QueueDel(blindfold);
                _seekerBlindfolds.Remove(netEnt);
            }

            if (TryComp<ActorComponent>(uid.Value, out var actor))
                RaiseNetworkEvent(new ArenaSeekerUnfreezeEvent(),

                Filter.SinglePlayer(actor.PlayerSession));
        }
    }

    private void StartIntermission()
    {
        _seekerNetEntities.Clear();
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

    private bool CheckPropHuntWinCondition()
    {
        if (CurrentMode != ArenaMode.PropHunt || RoundState != ArenaRoundState.Active)
            return false;

        foreach (var netEnt in _roster)
        {
            if (_seekerNetEntities.Contains(netEnt))
                continue;

            if (TryGetEntity(netEnt, out var uid) && !HasComp<ActorComponent>(uid.Value))
                continue;

            return false;
        }

        Log.Info("Prop Hunt: all hiders eliminated, seekers win!");
        _chat.ChatMessageToAll(ChatChannel.Server,
            Loc.GetString("arena-winner-prophunt-seekers"),
            Loc.GetString("arena-winner-prophunt-seekers-wrap"),
            EntityUid.Invalid, false, true, Color.OrangeRed);
        StartIntermission();
        return true;
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
        if (_roster.Count >= 2)
            available.Add(ArenaMode.PropHunt);

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
            case ArenaMode.PropHunt:
            {
                // Timer expired — hiders win
                _chat.ChatMessageToAll(ChatChannel.Server,
                    Loc.GetString("arena-winner-prophunt-hiders"),
                    Loc.GetString("arena-winner-prophunt-hiders-wrap"),
                    EntityUid.Invalid, false, true, Color.OrangeRed);
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
        var phVotes = _votes.Values.Count(v => v == ArenaMode.PropHunt);
        var tdmVotes = _votes.Values.Count(v => v == ArenaMode.TDM);

        if (tdmVotes > dmVotes && tdmVotes > phVotes)
            NextMode = ArenaMode.TDM;
        else if (phVotes > dmVotes && _roster.Count >= 2)
            NextMode = ArenaMode.PropHunt;
        else
            NextMode = ArenaMode.Deathmatch;

        _votes.Clear();
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
                if (CurrentMode == ArenaMode.PropHunt)
                    StartPropHunt();
                else if (CurrentMode == ArenaMode.TDM)
                    StartTDM();
                else
                    StartDeathmatch();
                break;
            case ArenaRoundState.Hiding:
                StartPropHuntHunt();
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

    private void RefreshPresets()
    {
        _presets.Clear();
        foreach (var p in _protos.EnumeratePrototypes<ArenaLoadoutPresetPrototype>())
            _presets.Add(p);
    }

    private void OnJoin(ArenaJoinEvent msg, EntitySessionEventArgs args)
    {
        var who = (ICommonSession)args.SenderSession;

        if (!Enabled)
            return;

        if (who.AttachedEntity is not { Valid: true } ghost || !HasComp<GhostComponent>(ghost))
            return;

        if (_activeEuis.ContainsKey(who))
            return;

        if (CurrentMode == ArenaMode.PropHunt && RoundState != ArenaRoundState.Intermission)
            return;

        if (CurrentMode == ArenaMode.TDM && RoundState != ArenaRoundState.Intermission)
        {
            // During TDM, joining is only allowed for new ghosts during intermission
            if (_roster.Contains(GetNetEntity(ghost)))
                return;
        }

        if (_presets.Count == 0)
            RefreshPresets();

        var eui = new ArenaLoadoutEui(this, who, ghost);
        _eui.OpenEui(eui, who);
        _activeEuis[who] = eui;
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

    private void OnDeath(MobStateChangedEvent ev)
    {
        if (ev.NewMobState != MobState.Dead)
            return;

        if (!TryComp<ArenaPlayerComponent>(ev.Target, out var arenaPlayer) ||
            !_roster.Contains(GetNetEntity(ev.Target)))
            return;

        var victimNet = GetNetEntity(ev.Target);

        // Find killer and track kills
        if (TryGetKillerMind(ev.Origin, out var killerMind, out _) &&
            killerMind?.UserId is { } killerUserId)
        {
            if (CurrentMode == ArenaMode.TDM &&
                killerMind.OwnedEntity is { } killerEnt &&
                TryComp<ArenaPlayerComponent>(killerEnt, out var killerArena) &&
                killerArena.Team != ArenaTeam.None)
            {
                _tdmTeamKills[killerArena.Team]++;
                // Persistent
                _persistTdmKills.TryAdd(killerUserId, 0);
                _persistTdmKills[killerUserId]++;
                if (killerArena.Team == ArenaTeam.Blue)
                    _persistTdmBlueKills++;
                else
                    _persistTdmRedKills++;
            }
            else if (CurrentMode == ArenaMode.Deathmatch)
            {
                _dmKills.TryAdd(killerUserId, 0);
                _dmKills[killerUserId]++;
                // Persistent
                _persistDmKills.TryAdd(killerUserId, 0);
                _persistDmKills[killerUserId]++;
            }

            // Cache player name
            if (!_persistPlayerNames.ContainsKey(killerUserId))
            {
                try { _persistPlayerNames[killerUserId] = _prefs.GetPreferences(killerUserId).SelectedCharacter?.Name ?? "Unknown"; }
                catch { _persistPlayerNames[killerUserId] = "Unknown"; }
            }
        }

        // Track death for Deathmatch K/D
        if (CurrentMode == ArenaMode.Deathmatch)
        {
            _minds.TryGetMind(ev.Target, out var victimMindId, out var victimMind);
            if (victimMind?.UserId is { } victimUserId)
            {
                _dmDeaths.TryAdd(victimUserId, 0);
                _dmDeaths[victimUserId]++;
                _persistDmDeaths.TryAdd(victimUserId, 0);
                _persistDmDeaths[victimUserId]++;
            }
        }
        // Track death for TDM
        if (CurrentMode == ArenaMode.TDM)
        {
            _minds.TryGetMind(ev.Target, out var victimMindId, out var victimMind);
            if (victimMind?.UserId is { } victimUserId)
            {
                _persistTdmDeaths.TryAdd(victimUserId, 0);
                _persistTdmDeaths[victimUserId]++;
            }
        }

        if (CurrentMode == ArenaMode.PropHunt && RoundState == ArenaRoundState.Active)
        {
            if (!_seekerNetEntities.Contains(victimNet))
            {
                _roster.Remove(victimNet);
                _seekerNetEntities.Remove(victimNet);
                RestorePlayer(ev.Target, arenaPlayer);
                CheckPropHuntWinCondition();
                return;
            }
        }

        _playerTeams.Remove(victimNet);
        RestorePlayer(ev.Target, arenaPlayer);
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
        if (TryComp<TransformComponent>(origin.Value, out var xform))
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

        // Prop Hunt: if a seeker ghosts, reassign a new seeker
        if (CurrentMode == ArenaMode.PropHunt &&
            _seekerNetEntities.Contains(netEnt) &&
            _roster.Count - _seekerNetEntities.Count > 0)
        {
            _seekerNetEntities.Remove(netEnt);
            AssignNewSeeker();
        }

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

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        if (_persistDmKills.Count == 0 && _persistTdmKills.Count == 0)
            return;

        // Build DM player records sorted by K/D
        var dmPlayers = new List<ArenaPlayerRecord>();
        foreach (var (userId, kills) in _persistDmKills)
        {
            var deaths = _persistDmDeaths.GetValueOrDefault(userId, 0);
            dmPlayers.Add(new ArenaPlayerRecord
            {
                PlayerName = _persistPlayerNames.GetValueOrDefault(userId, "Unknown"),
                Kills = kills,
                Deaths = deaths,
                KD = deaths == 0 ? kills : (double)kills / deaths,
            });
        }
        dmPlayers = dmPlayers.OrderByDescending(p => p.KD).Take(10).ToList();

        // Build TDM player records
        var tdmPlayers = new List<ArenaPlayerRecord>();
        foreach (var (userId, kills) in _persistTdmKills)
        {
            var deaths = _persistTdmDeaths.GetValueOrDefault(userId, 0);
            tdmPlayers.Add(new ArenaPlayerRecord
            {
                PlayerName = _persistPlayerNames.GetValueOrDefault(userId, "Unknown"),
                Kills = kills,
                Deaths = deaths,
                KD = deaths == 0 ? kills : (double)kills / deaths,
            });
        }
        tdmPlayers = tdmPlayers.OrderByDescending(p => p.KD).Take(10).ToList();

        // Determine best TDM team by total kills
        ArenaTeam? bestTeam = _persistTdmBlueKills > _persistTdmRedKills ? ArenaTeam.Blue :
                              _persistTdmRedKills > _persistTdmBlueKills ? ArenaTeam.Red : null;

        // Find overall best player across both modes
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

        var manifest = new ArenaManifestEvent
        {
            DmPlayers = dmPlayers,
            TdmPlayers = tdmPlayers,
            BestTdmTeam = bestTeam,
            OverallBest = overallBest,
        };
        RaiseNetworkEvent(manifest);
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
        _seekerNetEntities.Clear();
        _playerTeams.Clear();
        _blueSpawns.Clear();
        _redSpawns.Clear();
        _tdmDoors.Clear();
        _arenaMap = null;
        _persistDmKills.Clear();
        _persistDmDeaths.Clear();
        _persistTdmKills.Clear();
        _persistTdmDeaths.Clear();
        _persistPlayerNames.Clear();
        _persistTdmBlueKills = 0;
        _persistTdmRedKills = 0;
        RoundStarted = false;
        RoundState = ArenaRoundState.Intermission;
        RoundTimeRemaining = IntermissionDuration;
        CurrentMode = ArenaMode.Deathmatch;
    }

    public ArenaLoadoutEuiState GetLoadoutState()
    {
        if (_presets.Count == 0)
            RefreshPresets();

        var options = new List<ArenaLoadoutOption>();
        for (var i = 0; i < _presets.Count; i++)
        {
            var p = _presets[i];

            // Filter presets by current mode
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

        return new ArenaLoadoutEuiState(options);
    }

    public bool SpawnPlayer(ArenaLoadoutEui eui, ICommonSession who, EntityUid sourceGhost, int kitIdx)
    {
        if (!Enabled)
            return false;

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

        var kitIdxClamped = Math.Clamp(kitIdx, 0, _presets.Count - 1);
        var preset = _presets[kitIdxClamped];

        // Determine spawn position based on mode and team
        EntityCoordinates spot;
        if (CurrentMode == ArenaMode.TDM)
        {
            var team = preset.Team;
            spot = GetTeamSpawn(team);
            _playerTeams[GetNetEntity(sourceGhost)] = team;
        }
        else
        {
            var sites = new List<EntityCoordinates>();
            var cursor = AllEntityQuery<ArenaSpawnPointComponent, TransformComponent>();
            while (cursor.MoveNext(out _, out _, out var where))
            {
                if (where.MapID == Transform(map).MapID)
                    sites.Add(where.Coordinates);
            }

            spot = sites.Count > 0
                ? _luck.Pick(sites)
                : new EntityCoordinates(map, System.Numerics.Vector2.Zero);
        }

        var profile = _prefs.GetPreferences(who.UserId).SelectedCharacter as HumanoidCharacterProfile;
        string speciesId = profile?.Species ?? SharedHumanoidAppearanceSystem.DefaultSpecies;
        var species = _protos.Index<SpeciesPrototype>(speciesId);
        var fresh = Spawn(species.Prototype, spot);

        if (profile != null)
            _humanoid.LoadProfile(fresh, profile);

        _meta.SetEntityName(fresh, who.Name);

        _stationSpawning.EquipStartingGear(fresh, preset, raiseEvent: false);

        var arenaPlayer = EnsureComp<ArenaPlayerComponent>(fresh);
        arenaPlayer.OriginalMind = originalMindId;
        arenaPlayer.OriginalGhost = sourceGhost;
        arenaPlayer.CanReturnToBody = ghost.CanReturnToBody;
        arenaPlayer.Team = preset.Team;
        EnsureComp<AntagImmuneComponent>(fresh);

        if (RoundState == ArenaRoundState.Intermission)
            EnsureComp<PacifiedComponent>(fresh);

        // The disposable arena body must never inherit the round mind's roles or objectives.
        _minds.SetUserId(originalMindId, null, originalMind);
        _minds.TransferTo(originalMindId, null, createGhost: false, mind: originalMind);
        var temporaryMind = _minds.CreateMind(who.UserId, who.Name);
        _minds.TransferTo(temporaryMind, fresh, mind: temporaryMind.Comp);
        QueueDel(sourceGhost);
        _roles.MindAddJobRole(temporaryMind, silent: true, jobPrototype: "ArenaWarrior");

        var netEnt = GetNetEntity(fresh);
        _roster.Add(netEnt);
        if (CurrentMode == ArenaMode.TDM && preset.Team != ArenaTeam.None)
            _playerTeams[netEnt] = preset.Team;

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

    private void AssignNewSeeker()
    {
        var hiders = _roster
            .Where(n => !_seekerNetEntities.Contains(n))
            .ToList();

        if (hiders.Count == 0)
            return;

        _luck.Shuffle(hiders);
        var newSeeker = hiders[0];
        _seekerNetEntities.Add(newSeeker);

        if (TryGetEntity(newSeeker, out var uid) &&
            TryComp<ActorComponent>(uid, out var actor))
        {
            EquipSeeker(uid.Value);
            var msg = Loc.GetString("arena-seeker-assigned");
            RaiseNetworkEvent(new ArenaSeekerNotifyEvent(msg),
                Filter.SinglePlayer(actor.PlayerSession));
        }

        Log.Info($"Prop Hunt: new seeker assigned");
    }

    private void EquipSeeker(EntityUid uid)
    {
        var sword = Spawn("EnergySwordDouble", Transform(uid).Coordinates);
        if (_inventory.TryGetSlotEntity(uid, "back", out var backpack) &&
            TryComp<StorageComponent>(backpack, out var storageComp) &&
            _storage.Insert(backpack.Value, sword, out _, storageComp: storageComp, playSound: false))
        {
            return;
        }
        _hands.TryPickupAnyHand(uid, sword);
    }

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
        _cleanTick += frameTime;
        if (_cleanTick >= 60f)
        {
            _cleanTick = 0f;
            ZapArena();
        }

        TickRound(frameTime);
    }

    private float _cleanTick;
}
