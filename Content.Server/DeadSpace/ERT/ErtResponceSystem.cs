// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Chat.Systems;
using Content.Server.DeadSpace.Languages;
using Content.Shared.DeadSpace.ERT;
using Content.Shared.DeadSpace.ERT.Prototypes;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Prototypes;
using Content.Server.DeadSpace.ERTCall;
using Content.Server.GameTicking.Rules;
using System.Linq;
using Content.Shared.Storage;
using Robust.Shared.Random;
using Robust.Shared.Map;
using Content.Shared.GameTicking.Components;
using Robust.Server.GameObjects;
using Content.Shared.Mind.Components;
using Content.Shared.GameTicking;
using Content.Server.Chat.Managers;
using Content.Server.AlertLevel;
using Content.Shared.Administration.Logs;
using Content.Shared.DeadSpace.ERT;
using Content.Shared.Database;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.ERT;

// Работает для одной станции, потому что пока нет смысла делать для множества
public sealed class ErtResponceSystem : SharedErtResponceSystem
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TimedWindowSystem _timedWindowSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly Dictionary<ProtoId<ErtTeamPrototype>, TimedWindow> _expectedTeams = new();
    private TimedWindow? _coolDown = null;
    private readonly TimedWindow _defaultWindowWaitingSpecies = new(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    private List<WaitingSpeciesSettings> _windowWaitingSpecies = new();

    /// <summary>
    ///     Сумма очков для заказа обр, доступная в начале каждого раунда.
    /// </summary>
    private const int DefaultPoints = 8;
    /// <summary>
    ///     Текущий баланс очков.
    /// </summary>
    private int _points = DefaultPoints;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RequestErtAdminStateMessage>(OnRequestErtAdminState);
        SubscribeNetworkEvent<AdminModifyErtEntryMessage>(OnAdminModifyErtEntry);
        SubscribeNetworkEvent<AdminSetPointsMessage>(OnAdminSetPoints);
        SubscribeNetworkEvent<AdminDeleteErtMessage>(OnDeleteErt);
        SubscribeNetworkEvent<AdminSetCooldownMessage>(OnAdminSetCooldown);

        SubscribeLocalEvent<ErtSpawnRuleComponent, RuleLoadedGridsEvent>(OnRuleLoadedGrids);
        SubscribeLocalEvent<ErtSpeciesRoleComponent, MindAddedMessage>(OnMindAdded);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRequestErtAdminState(RequestErtAdminStateMessage msg, EntitySessionEventArgs args)
    {
        var entries = new List<ErtAdminEntry>();

        foreach (var (teamId, window) in _expectedTeams)
        {
            if (!_prototypeManager.TryIndex(teamId, out var proto))
                continue;

            var seconds = _timedWindowSystem.GetSecondsRemaining(window);

            entries.Add(new ErtAdminEntry(teamId.ToString(), proto.Name, seconds, proto.Price));
        }

        var cooldownSeconds = 0;
        if (_coolDown != null && !_timedWindowSystem.IsExpired(_coolDown))
        {
            cooldownSeconds = _timedWindowSystem.GetSecondsRemaining(_coolDown);
        }

        var response = new ErtAdminStateResponse(entries.ToArray(), _points, cooldownSeconds);
        RaiseNetworkEvent(response, args.SenderSession.Channel);
    }

    private void OnAdminModifyErtEntry(AdminModifyErtEntryMessage msg, EntitySessionEventArgs args)
    {
        var key = new ProtoId<ErtTeamPrototype>(msg.ProtoId);

        if (!_expectedTeams.TryGetValue(key, out var window))
        {
            RaiseNetworkEvent(new ErtAdminActionResult(false, "No expected team with that id"), args.SenderSession.Channel);
            return;
        }

        // Устанавливаем абсолютное время ожидания
        window.Remaining = _timing.CurTime + TimeSpan.FromSeconds(msg.Seconds);

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"Admin {args.SenderSession.Name} set ERT '{msg.ProtoId}' arrival to {msg.Seconds} seconds");

        _chatManager.SendAdminAlert($"Админ {args.SenderSession.Name} изменил время прибытия ОБР '{msg.ProtoId}' на {msg.Seconds} сек.");

        RaiseNetworkEvent(new ErtAdminActionResult(true, "OK"), args.SenderSession.Channel);
    }

    private void OnAdminSetPoints(AdminSetPointsMessage msg, EntitySessionEventArgs args)
    {
        _points = msg.Points;

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"Admin {args.SenderSession.Name} set ERT points to {_points}");

        _chatManager.SendAdminAlert($"Админ {args.SenderSession.Name} установил баланс ОБР на {_points} очков.");

        RaiseNetworkEvent(new ErtAdminActionResult(true, "OK"), args.SenderSession.Channel);
    }

    private void OnDeleteErt(AdminDeleteErtMessage msg, EntitySessionEventArgs args)
    {
        var key = new ProtoId<ErtTeamPrototype>(msg.ProtoId);

        _expectedTeams.Remove(key);
        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"Admin {args.SenderSession.Name} delete ERT {msg.ProtoId}");

        _chatManager.SendAdminAlert($"Админ {args.SenderSession.Name} удалил отряд {msg.ProtoId} из списка ожиданий.");

        RaiseNetworkEvent(new ErtAdminActionResult(true, "OK"), args.SenderSession.Channel);
    }

    private void OnAdminSetCooldown(AdminSetCooldownMessage msg, EntitySessionEventArgs args)
    {
        // create a fixed cooldown window of given seconds
        var window = new TimedWindow(TimeSpan.FromSeconds(msg.Seconds), TimeSpan.FromSeconds(msg.Seconds));
        _timedWindowSystem.Reset(window);
        _coolDown = window;

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"Admin {args.SenderSession.Name} set ERT cooldown to {msg.Seconds} seconds");

        _chatManager.SendAdminAlert($"Админ {args.SenderSession.Name} установил откат ОБР на {msg.Seconds} сек.");

        RaiseNetworkEvent(new ErtAdminActionResult(true, "OK"), args.SenderSession.Channel);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _windowWaitingSpecies.Clear();
        _expectedTeams.Clear();
        _points = DefaultPoints;
    }

    private void OnMindAdded(Entity<ErtSpeciesRoleComponent> ent, ref MindAddedMessage args)
    {
        if (ent.Comp.Settings == null)
            return;

        _windowWaitingSpecies.Remove(ent.Comp.Settings);

        if (!_prototypeManager.TryIndex(ent.Comp.Settings.TeamId, out var prototype))
            return;

        if (!EntityManager.EntityExists(ent.Comp.Settings.SpawnPoint))
            return;

        var spawns = EntitySpawnCollection.GetSpawns(prototype.Spawns, _random);

        foreach (var proto in spawns)
        {
            Spawn(proto, Transform(ent.Comp.Settings.SpawnPoint).Coordinates);
        }
    }

    private void OnRuleLoadedGrids(Entity<ErtSpawnRuleComponent> ent, ref RuleLoadedGridsEvent args)
    {
        if (!_prototypeManager.TryIndex(ent.Comp.Team, out var prototype))
            return;

        var query = EntityQueryEnumerator<ErtSpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.MapID != args.Map)
                continue;

            if (xform.GridUid is not { } grid || !args.Grids.Contains(grid))
                continue;

            if (prototype.Special != null)
            {
                var spec = Spawn(prototype.Special.Value, Transform(uid).Coordinates);

                var window = _defaultWindowWaitingSpecies.Clone();
                var settings = new WaitingSpeciesSettings(args.Map, window, ent.Comp.Team, uid);

                EnsureComp<ErtSpeciesRoleComponent>(spec).Settings = settings;
                _timedWindowSystem.Reset(window);

                _windowWaitingSpecies.Add(settings);
                return;
            }

            var spawns = EntitySpawnCollection.GetSpawns(prototype.Spawns, _random);

            foreach (var proto in spawns)
            {
                Spawn(proto, Transform(uid).Coordinates);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        for (var i = _windowWaitingSpecies.Count - 1; i >= 0; i--)
        {
            var settings = _windowWaitingSpecies[i];

            if (!_timedWindowSystem.IsExpired(settings.Window))
                continue;

            _windowWaitingSpecies.RemoveAt(i);
            _mapSystem.DeleteMap(settings.MapId);

            if (!_prototypeManager.TryIndex(settings.TeamId, out var prototype))
                continue;

            if (prototype.CancelMessage != null)
            {
                _chatSystem.DispatchGlobalAnnouncement(
                    message: prototype.CancelMessage,
                    sender: Loc.GetString("chat-manager-sender-announcement"),
                    colorOverride: Color.FromHex("#1d8bad"),
                    playSound: true,
                    usePresetTTS: true,
                    languageId: LanguageSystem.DefaultLanguageId
                );
            }
        }

        foreach (var (team, window) in _expectedTeams.ToArray())
        {
            if (!_timedWindowSystem.IsExpired(window))
                continue;

            EnsureErtTeam(team);
            _expectedTeams.Remove(team);
        }
    }

    public bool TryCallErt(
    ProtoId<ErtTeamPrototype> team,
    EntityUid? station,
    out string? reason,
    bool toPay = true,
    bool needCooldown = true,
    bool needWarn = true)
    {
        reason = null;

        if (!_prototypeManager.TryIndex(team, out var prototype))
        {
            reason = Loc.GetString("ert-call-fail-prototype-missing");
            return false;
        }

        if (station != null && prototype.CodeBlackList != null)
        {
            var level = _alertLevelSystem.GetLevel(station.Value);
            if (prototype.CodeBlackList.Contains(level))
            {
                reason = Loc.GetString(
                    "ert-call-fail-code-blacklist",
                    ("level", level)
                );
                return false;
            }
        }

        if (needCooldown)
        {
            if (_coolDown != null && !_timedWindowSystem.IsExpired(_coolDown))
            {
                var seconds = _timedWindowSystem.GetSecondsRemaining(_coolDown);

                reason = Loc.GetString(
                    "ert-call-fail-cooldown",
                    ("seconds", seconds)
                );
                return false;
            }
            else
            {
                var cooldown = prototype.Cooldown.Clone();
                _timedWindowSystem.Reset(cooldown);
                _coolDown = cooldown;
            }
        }

        if (toPay)
        {
            if (prototype.Price > _points)
            {
                reason = Loc.GetString(
                    "ert-call-fail-not-enough-points",
                    ("price", prototype.Price),
                    ("balance", _points)
                );
                return false;
            }

            _points -= prototype.Price;
        }

        if (needWarn)
        {
            _chatSystem.DispatchGlobalAnnouncement(
                message: string.IsNullOrEmpty(prototype.Notification) ? Loc.GetString("ert-responce-caused-messager", ("team", prototype.Name)) : Loc.GetString(prototype.Notification),
                sender: string.IsNullOrEmpty(prototype.Sender) ? Loc.GetString("chat-manager-sender-announcement") : Loc.GetString(prototype.Sender),
                colorOverride: Color.FromHex("#1d8bad"),
                playSound: true,
                usePresetTTS: true,
                languageId: LanguageSystem.DefaultLanguageId
            );
        }

        var window = prototype.TimeWindowToSpawn.Clone();
        _timedWindowSystem.Reset(window);

        _expectedTeams.Add(team, window);

        return true;
    }

    public EntityUid? EnsureErtTeam(ProtoId<ErtTeamPrototype> team)
    {
        if (!_prototypeManager.TryIndex(team, out var prototype))
            return null;

        var ruleEntity = Spawn(prototype.ErtRule, MapCoordinates.Nullspace);
        EnsureComp<ErtSpawnRuleComponent>(ruleEntity).Team = team;

        // не нужен в _allPreviousGameRules, потому что сам по себе не является правилом
        var ev = new GameRuleAddedEvent(ruleEntity, prototype.ErtRule);
        RaiseLocalEvent(ruleEntity, ref ev, true);

        return ruleEntity;
    }

    public int GetBalance()
    {
        return _points;
    }

}

public sealed class WaitingSpeciesSettings
{
    public MapId MapId;
    public TimedWindow Window;
    public ProtoId<ErtTeamPrototype> TeamId;
    public EntityUid SpawnPoint;

    public WaitingSpeciesSettings(MapId mapId, TimedWindow window, ProtoId<ErtTeamPrototype> teamId, EntityUid spawnPoint)
    {
        MapId = mapId;
        Window = window;
        TeamId = teamId;
        SpawnPoint = spawnPoint;
    }
}