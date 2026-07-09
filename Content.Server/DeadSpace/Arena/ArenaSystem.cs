using Content.Server.EUI;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Shared.Body.Part;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Fluids.Components;
using Content.Shared.Ghost;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Station;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.EntitySerialization.Systems;
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

    private const string ArenaMapFile = "/Maps/_DeadSpace/arena.yml";

    private EntityUid? _arenaMap;
    private readonly HashSet<NetEntity> _roster = new();
    private readonly List<ArenaLoadoutPresetPrototype> _presets = new();

    public override void Initialize()
    {
        SubscribeNetworkEvent<ArenaJoinEvent>(OnJoin);
        SubscribeNetworkEvent<ArenaLeaveEvent>(OnLeave);
        SubscribeLocalEvent<MobStateChangedEvent>(OnDeath);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
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

        if (who.AttachedEntity is { } body && _roster.Contains(GetNetEntity(body)))
            return;

        if (_presets.Count == 0)
            RefreshPresets();

        var eui = new ArenaLoadoutEui(this, who);
        _eui.OpenEui(eui, who);
    }

    private void OnLeave(ArenaLeaveEvent msg, EntitySessionEventArgs args)
    {
        var who = (ICommonSession)args.SenderSession;
        RemovePlayer(who);
    }

    private void OnDeath(MobStateChangedEvent ev)
    {
        if (ev.NewMobState != MobState.Dead)
            return;

        var token = GetNetEntity(ev.Target);
        if (!_roster.Remove(token))
            return;

        if (TryComp<ActorComponent>(ev.Target, out var actor))
            Evacuate((ICommonSession)actor.PlayerSession);
        else
            QueueDel(ev.Target);
    }

    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        var token = GetNetEntity(ev.Entity);
        if (!_roster.Remove(token))
            return;

        QueueDel(ev.Entity);
    }

    public ArenaLoadoutEuiState GetLoadoutState()
    {
        if (_presets.Count == 0)
            RefreshPresets();

        var options = new List<ArenaLoadoutOption>();
        for (var i = 0; i < _presets.Count; i++)
        {
            var p = _presets[i];
            options.Add(new ArenaLoadoutOption
            {
                Index = i,
                Name = Loc.GetString(p.NameLoc),
                Description = Loc.GetString(p.DescLoc),
                Category = Loc.GetString(p.Category),
                SpritePrototype = p.IconPrototype,
            });
        }

        return new ArenaLoadoutEuiState(options);
    }

    public void SpawnPlayer(ICommonSession who, int kitIdx)
    {
        EnsureMap();

        if (_arenaMap is not { } map)
            return;

        if (!_minds.TryGetMind(who, out var mindId, out var mind))
            return;

        var old = who.AttachedEntity;
        if (old != null && HasComp<GhostComponent>(old.Value))
        {
            _minds.TransferTo(mindId, null, mind: mind);
            QueueDel(old.Value);
        }

        if (_presets.Count == 0)
            RefreshPresets();

        var sites = new List<EntityCoordinates>();
        var cursor = AllEntityQuery<ArenaSpawnPointComponent, TransformComponent>();
        while (cursor.MoveNext(out _, out _, out var where))
        {
            if (where.MapID == Transform(map).MapID)
                sites.Add(where.Coordinates);
        }

        var spot = sites.Count > 0
            ? _luck.Pick(sites)
            : new EntityCoordinates(map, System.Numerics.Vector2.Zero);

        var fresh = Spawn("MobHuman", spot);

        _meta.SetEntityName(fresh, who.Name);

        if (_presets.Count > 0)
        {
            var idx = Math.Clamp(kitIdx, 0, _presets.Count - 1);
            _stationSpawning.EquipStartingGear(fresh, _presets[idx], raiseEvent: false);
        }

        _minds.TransferTo(mindId, fresh, mind: mind);

        var arenaPlayer = EnsureComp<ArenaPlayerComponent>(fresh);
        _roster.Add(GetNetEntity(fresh));
    }

    private void RemovePlayer(ICommonSession who)
    {
        var body = who.AttachedEntity;
        if (body != null)
            _roster.Remove(GetNetEntity(body.Value));

        Evacuate(who);
    }

    private void Evacuate(ICommonSession who)
    {
        var body = who.AttachedEntity;
        if (body == null)
            return;

        _roster.Remove(GetNetEntity(body.Value));

        if (_minds.TryGetMind(who, out var mindId, out var mind))
            _ghosts.SpawnGhost((mindId, mind), null, false);

        QueueDel(body.Value);
    }

    private void EnsureMap()
    {
        if (_arenaMap != null && Exists(_arenaMap.Value))
            return;

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

            if (HasComp<ActorComponent>(thing))
                continue;

            if (HasComp<GhostComponent>(thing))
                continue;

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
        if (_cleanTick < 60f)
            return;

        _cleanTick = 0f;
        ZapArena();
    }

    private float _cleanTick;
}
