// TO-DO -- Сделать режим оригинальнее
using Content.Server.Clothing.Systems;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Fluids.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Mobs;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.DeadSpace.Arena;

public sealed class ArenaRuleSystem : GameRuleSystem<ArenaRuleComponent>
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly GhostSystem _ghostSystem = default!;
    [Dependency] private readonly OutfitSystem _outfitSystem = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;

    private const string ArenaMapPath = "/Maps/_DeadSpace/arena.yml";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArenaPlayerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ArenaPlayerComponent, PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeNetworkEvent<ArenaJoinRequestEvent>(OnArenaJoinRequest);
        SubscribeNetworkEvent<ArenaLeaveRequestEvent>(OnArenaLeaveRequest);
    }

    protected override void Added(EntityUid uid, ArenaRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        var opts = DeserializationOptions.Default with { InitializeMaps = true };
        var mapPath = new ResPath(ArenaMapPath);

        if (_mapLoader.TryLoadMap(mapPath, out var mapId, out var grids, opts))
        {
            component.ArenaMap = mapId.Value.Owner;
            Log.Info($"Arena map loaded: {component.ArenaMap}");
        }
        else
        {
            Log.Error($"Failed to load arena map from {ArenaMapPath}");
            var newMapId = _mapManager.CreateMap();
            component.ArenaMap = _mapManager.GetMapEntityId(newMapId);
            Log.Warning($"Created empty map {newMapId} as fallback");
        }
    }

    protected override void Started(EntityUid uid, ArenaRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        component.Active = true;
        Log.Info("Arena started");
    }

    protected override void Ended(EntityUid uid, ArenaRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        component.Active = false;

        foreach (var netEntity in component.Players)
        {
            if (!TryGetEntity(netEntity, out var playerEnt))
                continue;

            if (TryComp<ActorComponent>(playerEnt, out var actor))
            {
                RemovePlayerFromArena(actor.PlayerSession, component);
            }

            QueueDel(playerEnt.Value);
        }

        component.Players.Clear();

        if (component.ArenaMap is { } mapUid)
        {
            _mapManager.DeleteMap(Transform(mapUid).MapID);
            component.ArenaMap = null;
        }

        Log.Info("Arena ended, cleaned up");
    }

    protected override void ActiveTick(EntityUid uid, ArenaRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        if (!component.Active || component.ArenaMap is not { } mapUid)
            return;

        var curTime = Timing.CurTime;
        if (curTime < component.NextCleanup)
            return;

        component.NextCleanup = curTime + component.CleanupInterval;

        var mapId = Transform(mapUid).MapID;
        var enumerator = AllEntityQuery<TransformComponent>();
        while (enumerator.MoveNext(out var ent, out var xform))
        {
            if (!xform.ParentUid.IsValid() || xform.MapID != mapId)
                continue;

            if (HasComp<MapGridComponent>(ent))
                continue;

            if (!HasComp<MapGridComponent>(xform.ParentUid) && xform.ParentUid != mapUid)
                continue;

            if (HasComp<PuddleComponent>(ent) || (!xform.Anchored && !HasComp<ActorComponent>(ent)))
                QueueDel(ent);
        }
    }

    private void OnArenaJoinRequest(ArenaJoinRequestEvent msg, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;

        var ruleEnt = GetArenaRuleEntity();
        if (ruleEnt == null)
        {
            GameTicker.StartGameRule("ArenaRule");
            ruleEnt = GetArenaRuleEntity();
        }

        if (ruleEnt == null)
            return;

        if (!TryComp<ArenaRuleComponent>(ruleEnt.Value, out var rule) || !rule.Active)
            return;

        if (HasComp<ArenaPlayerComponent>(session.AttachedEntity))
            return;

        var eui = new ArenaLoadoutEui(this, ruleEnt.Value, session);
        _euiManager.OpenEui(eui, session);
    }

    private void OnArenaLeaveRequest(ArenaLeaveRequestEvent msg, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;
        var ruleEnt = GetArenaRuleEntity();
        if (ruleEnt == null)
            return;

        if (!TryComp<ArenaRuleComponent>(ruleEnt.Value, out var rule))
            return;

        RemovePlayerFromArena(session, rule);
    }

    public void SpawnPlayer(ICommonSession session, EntityUid ruleEntity, int weaponIndex)
    {
        if (!TryComp<ArenaRuleComponent>(ruleEntity, out var rule))
            return;

        if (rule.ArenaMap is not { } mapUid)
            return;

        if (!_mindSystem.TryGetMind(session, out var mindId, out var mind))
            return;

        var attached = session.AttachedEntity;
        if (attached != null && HasComp<GhostComponent>(attached.Value))
        {
            _mindSystem.TransferTo(mindId, null, mind: mind);
            QueueDel(attached.Value);
        }

        var coords = new EntityCoordinates(mapUid, new System.Numerics.Vector2(0, 0));
        var humanoid = Spawn("MobHuman", coords);

        _metadata.SetEntityName(humanoid, session.Name);
        _mindSystem.TransferTo(mindId, humanoid, mind: mind);

        var playerComp = EnsureComp<ArenaPlayerComponent>(humanoid);
        playerComp.RuleEntity = ruleEntity;

        ApplyLoadout(humanoid, rule, weaponIndex);

        var netEntity = GetNetEntity(humanoid);
        rule.Players.Add(netEntity);

        Dirty(ruleEntity, rule);
    }

    public ArenaLoadoutEuiState GetLoadoutState(ArenaRuleComponent component)
    {
        var options = new List<ArenaLoadoutOption>();
        for (var i = 0; i < component.WeaponLoadouts.Count; i++)
        {
            var loadout = component.WeaponLoadouts[i];
            options.Add(new ArenaLoadoutOption
            {
                Index = i,
                Name = Loc.GetString(loadout.Name),
                Description = Loc.GetString(loadout.Description),
                Category = Loc.GetString(loadout.Category),
                SpritePrototype = loadout.Sprite
            });
        }

        return new ArenaLoadoutEuiState(options);
    }

    private EntityUid? GetArenaRuleEntity()
    {
        var query = EntityQueryEnumerator<ArenaRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var _, out var gameRule))
        {
            if (GameTicker.IsGameRuleActive(uid, gameRule))
                return uid;
        }

        return null;
    }

    private void ApplyLoadout(EntityUid humanoid, ArenaRuleComponent rule, int weaponIndex)
    {
        var gearId = rule.Gear;

        if (weaponIndex >= 0 && weaponIndex < rule.WeaponLoadouts.Count)
        {
            var weaponGear = rule.WeaponLoadouts[weaponIndex].Gear;
            if (!string.IsNullOrEmpty(weaponGear))
                gearId = weaponGear;
        }

        _outfitSystem.SetOutfit(humanoid, gearId);
    }

    private void OnMobStateChanged(Entity<ArenaPlayerComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        QueueDel(ent);
    }

    private void OnPlayerDetached(Entity<ArenaPlayerComponent> ent, ref PlayerDetachedEvent args)
    {
        QueueDel(ent);
    }

    private void RemovePlayerFromArena(ICommonSession session, ArenaRuleComponent rule)
    {
        var attached = session.AttachedEntity;
        if (attached == null || !TryComp<ArenaPlayerComponent>(attached.Value, out var player))
            return;

        var netEnt = GetNetEntity(attached.Value);
        rule.Players.Remove(netEnt);

        if (player.RuleEntity is { } ruleEnt)
        {
            Dirty(ruleEnt, rule);
        }

        RemComp<ArenaPlayerComponent>(attached.Value);
        QueueDel(attached.Value);

        if (_mindSystem.TryGetMind(session, out var mindId, out var mind))
        {
            _ghostSystem.SpawnGhost((mindId, mind), null, false);
        }
    }


}
