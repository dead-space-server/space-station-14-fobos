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

namespace Content.Server.DeadSpace.ERT;

// Работает для одной станции, потому что пока нет смысла делать для множества
public sealed class ErtResponceSystem : SharedErtResponceSystem
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TimedWindowSystem _timedWindowSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    private ProtoId<ErtTeamPrototype>? _expectedTeam = null;
    private TimedWindow? _windowWaitingArrival = null;
    private readonly TimedWindow _defaultWindowWaitingSpecies = new(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    private List<WaitingSpeciesSettings> _windowWaitingSpecies = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ErtSpawnRuleComponent, RuleLoadedGridsEvent>(OnRuleLoadedGrids);
        SubscribeLocalEvent<ErtSpeciesRoleComponent, MindAddedMessage>(OnMindAdded);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _windowWaitingSpecies.Clear();
        _windowWaitingArrival = null;
        _expectedTeam = null;
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

        if (_expectedTeam == null)
            return;

        if (_windowWaitingArrival != null && _timedWindowSystem.IsExpired(_windowWaitingArrival))
            EnsureErtTeam(_expectedTeam.Value);
    }

    public bool TryCallErt(ProtoId<ErtTeamPrototype> team)
    {
        if (_expectedTeam != null)
            return false;

        if (!_prototypeManager.TryIndex(team, out var prototype))
            return false;

        _chatSystem.DispatchGlobalAnnouncement(
            message: Loc.GetString("ert-responce-caused-messager", ("team", prototype.Name)),
            sender: Loc.GetString("chat-manager-sender-announcement"), // На всякий
            colorOverride: Color.FromHex("#1d8bad"),
            playSound: true,
            usePresetTTS: true,
            languageId: LanguageSystem.DefaultLanguageId
        );

        _expectedTeam = team;
        _windowWaitingArrival = prototype.TimeWindowToSpawn;
        _timedWindowSystem.Reset(_windowWaitingArrival);

        return true;
    }

    public void EnsureErtTeam(ProtoId<ErtTeamPrototype> team)
    {
        if (!_prototypeManager.TryIndex(team, out var prototype))
            return;

        _expectedTeam = null;
        _windowWaitingArrival = null;

        var ruleEntity = Spawn(prototype.ErtRule, MapCoordinates.Nullspace);

        EnsureComp<ErtSpawnRuleComponent>(ruleEntity).Team = team;

        // не нужен в _allPreviousGameRules, потому что сам по себе не является правилом
        var ev = new GameRuleAddedEvent(ruleEntity, prototype.ErtRule);
        RaiseLocalEvent(ruleEntity, ref ev, true);
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