using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using Content.Server.Chat.Managers;
using Content.Server.Database;
using Content.Server.DeadSpace.Prison.Components;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Roles;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.DeadSpace.CCCCVars;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Throwing;
using Robust.Server.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Prison;

public sealed class PrisonSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly IServerPreferencesManager _preferences = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly StationSpawningSystem _spawning = default!;

    private readonly HashSet<NetUserId> _prisonUsers = [];
    private bool _enabled;

    private readonly TimeSpan _safeguardUpdateRate = TimeSpan.FromSeconds(10);
    private TimeSpan _nextSafeguardUpdate;

    private readonly TimeSpan _activeBanRefreshRate = TimeSpan.FromMinutes(1);
    private TimeSpan _nextActiveBanRefresh;
    private bool _activeBanRefreshRunning;

    public bool Enabled => _enabled;
    public bool Ready => _enabled && TryGetSpawnCoordinates(out _);

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, CCCCVars.PrisonEnabled, value => _enabled = value, true);

        SubscribeLocalEvent<PlayerJoinedLobbyEvent>(OnPlayerJoinedLobby);
        SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnPlayerBeforeSpawn);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<MindRoleAddAttemptEvent>(OnMindRoleAddAttempt);

        _player.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _player.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime >= _nextSafeguardUpdate)
        {
            _nextSafeguardUpdate = _timing.CurTime + _safeguardUpdateRate;
            SafeguardPrisoners();
        }

        if (_prisonUsers.Count == 0 ||
            _activeBanRefreshRunning ||
            _timing.CurTime < _nextActiveBanRefresh)
        {
            return;
        }

        _nextActiveBanRefresh = _timing.CurTime + _activeBanRefreshRate;
        RefreshActivePrisonBans();
    }

    public bool RegisterPrisonerConnection(NetUserId userId, IReadOnlyCollection<BanDef> bans)
    {
        if (IsUserCurrentlyAntagonist(userId))
            return false;

        if (!CanUsePrisonForBans(bans))
            return false;

        _prisonUsers.Add(userId);
        return true;
    }

    public bool CanUsePrisonForBans(IReadOnlyCollection<BanDef> bans)
    {
        if (!_enabled || !Ready || bans.Count == 0)
            return false;

        if (bans.Any(IsPermanentServerBan))
            return false;

        return bans.Any(IsTemporaryServerBan);
    }

    public bool TrySendToPrison(ICommonSession session, BanDef ban)
    {
        if (IsSessionAntagonist(session))
            return false;

        if (!_enabled || !Ready || !IsTemporaryServerBan(ban))
            return false;

        if (!TryGetSpawnCoordinates(out var coordinates))
            return false;

        _prisonUsers.Add(session.UserId);

        if (session.AttachedEntity is { } entity && Exists(entity) && !HasComp<GhostComponent>(entity))
        {
            SendEntityToPrison(entity, coordinates);
        }
        else if (session.Status == SessionStatus.InGame)
        {
            if (!TryGetHumanoidProfile(session, out var profile))
                return false;

            SpawnPrisonMob(session, profile, coordinates);
        }

        SendPrisonMessage(session, ban);
        return true;
    }

    public bool IsUserPrisoner(NetUserId userId)
    {
        if (_prisonUsers.Contains(userId))
            return true;

        return _player.TryGetSessionById(userId, out var session)
               && session.AttachedEntity is { } entity
               && HasComp<PrisonBoundComponent>(entity);
    }

    public bool IsEntityPrisoner(EntityUid entity)
    {
        if (HasComp<PrisonBoundComponent>(entity))
            return true;

        return _mind.TryGetMind(entity, out var mindId, out var mind)
               && IsMindPrisoner(mindId, mind);
    }

    public bool IsMindPrisoner(EntityUid mindId, MindComponent? mind = null)
    {
        return Resolve(mindId, ref mind, false)
               && mind.UserId is { } userId
               && IsUserPrisoner(userId);
    }

    private void OnPlayerJoinedLobby(PlayerJoinedLobbyEvent ev)
    {
        if (!IsUserPrisoner(ev.PlayerSession.UserId))
            return;

        _chat.DispatchServerMessage(ev.PlayerSession, Loc.GetString("prison-chat-join-message"));
    }

    private void OnPlayerBeforeSpawn(PlayerBeforeSpawnEvent ev)
    {
        if (!IsUserPrisoner(ev.Player.UserId))
            return;

        ev.Handled = true;

        if (!_enabled || !TryGetSpawnCoordinates(out var coordinates))
        {
            ev.Player.Channel.Disconnect(Loc.GetString("prison-unavailable-message"));
            return;
        }

        SpawnPrisonMob(ev.Player, ev.Profile, coordinates);
        _chat.DispatchServerMessage(ev.Player, Loc.GetString("prison-arrival-message"));
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        if (!IsUserPrisoner(ev.Player.UserId) && !HasComp<PrisonBoundComponent>(ev.Entity))
            return;

        _prisonUsers.Add(ev.Player.UserId);

        if (!_enabled || !TryGetSpawnCoordinates(out var coordinates))
        {
            ev.Player.Channel.Disconnect(Loc.GetString("prison-unavailable-message"));
            return;
        }

        if (HasComp<GhostComponent>(ev.Entity))
        {
            RemComp<PrisonBoundComponent>(ev.Entity);
            return;
        }

        var xform = Transform(ev.Entity);
        if (IsPrisonMap(xform.MapID))
            return;

        SendEntityToPrison(ev.Entity, coordinates);
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Disconnected)
            _prisonUsers.Remove(e.Session.UserId);
    }

    private void OnMindRoleAddAttempt(MindRoleAddAttemptEvent args)
    {
        if (!args.Antagonist || args.Mind.UserId is not { } userId || !IsUserPrisoner(userId))
            return;

        args.Cancel();

        if (_player.TryGetSessionById(userId, out var session))
            _chat.DispatchServerMessage(session, Loc.GetString("prison-antag-role-blocked"));
    }

    private void SpawnPrisonMob(ICommonSession session, HumanoidCharacterProfile profile, EntityCoordinates coordinates)
    {
        if (_mind.TryGetMind(session.UserId, out _, out var existingMind) && !existingMind.IsVisitingEntity)
            _mind.WipeMind(session);

        var newMind = _mind.CreateMind(session.UserId, profile.Name);
        _mind.SetUserId(newMind, session.UserId);

        var mob = _spawning.SpawnPlayerMob(coordinates, null, profile, null);
        _mind.TransferTo(newMind, mob);

        EnsureComp<PrisonBoundComponent>(mob);
        _prisonUsers.Add(session.UserId);
    }

    private bool TryGetHumanoidProfile(ICommonSession session, [NotNullWhen(true)] out HumanoidCharacterProfile? profile)
    {
        if (_preferences.TryGetCachedPreferences(session.UserId, out var preferences) &&
            preferences.SelectedCharacter is HumanoidCharacterProfile humanoid)
        {
            profile = humanoid;
            return true;
        }

        profile = null;
        return false;
    }

    private void SendEntityToPrison(EntityUid entity, EntityCoordinates coordinates)
    {
        DropInventory(entity);

        _transform.SetCoordinates(entity, coordinates);
        _transform.AttachToGridOrMap(entity);

        EnsureComp<PrisonBoundComponent>(entity);
    }

    private void DropInventory(EntityUid entity)
    {
        if (_inventory.TryGetContainerSlotEnumerator(entity, out var enumerator))
        {
            while (enumerator.NextItem(out var item, out var slot))
            {
                if (_inventory.TryUnequip(entity, entity, slot.Name, true, true))
                    _physics.ApplyAngularImpulse(item, ThrowingSystem.ThrowAngularImpulse);
            }
        }

        if (!TryComp(entity, out HandsComponent? hands))
            return;

        foreach (var hand in _hands.EnumerateHands((entity, hands)))
        {
            _hands.TryDrop((entity, hands), hand, checkActionBlocker: false, doDropInteraction: false);
        }
    }

    private void SafeguardPrisoners()
    {
        if (!_enabled || !TryGetSpawnCoordinates(out var coordinates))
            return;

        var query = EntityQueryEnumerator<PrisonBoundComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (HasComp<GhostComponent>(uid))
            {
                RemCompDeferred<PrisonBoundComponent>(uid);
                continue;
            }

            if (IsPrisonMap(xform.MapID))
                continue;

            SendEntityToPrison(uid, coordinates);
        }
    }

    private async void RefreshActivePrisonBans()
    {
        _activeBanRefreshRunning = true;

        try
        {
            var checks = new List<PrisonBanRefreshCheck>();

            foreach (var userId in _prisonUsers.ToArray())
            {
                if (!_player.TryGetSessionById(userId, out var session))
                {
                    _prisonUsers.Remove(userId);
                    continue;
                }

                checks.Add(CreateBanRefreshCheck(session));
            }

            if (checks.Count == 0)
                return;

            var results = new List<PrisonBanRefreshResult>();
            foreach (var check in checks)
            {
                var bans = await _db.GetBansAsync(
                    check.Address,
                    check.UserId,
                    check.HwId,
                    check.ModernHwIds,
                    includeUnbanned: false);

                results.Add(new PrisonBanRefreshResult(
                    check.UserId,
                    bans.FirstOrDefault(IsTemporaryServerBan),
                    bans.FirstOrDefault(IsPermanentServerBan)));
            }

            _taskManager.RunOnMainThread(() => ApplyActivePrisonBanRefresh(results));
        }
        catch (Exception e)
        {
            Log.Error($"Failed to refresh prison ban state: {e}");
        }
        finally
        {
            _activeBanRefreshRunning = false;
        }
    }

    private PrisonBanRefreshCheck CreateBanRefreshCheck(ICommonSession session)
    {
        var channel = session.Channel;
        ImmutableArray<byte>? hwId = channel.UserData.HWId;

        if (hwId.Value.Length == 0 || !_cfg.GetCVar(CCVars.BanHardwareIds))
            hwId = null;

        return new PrisonBanRefreshCheck(
            session.UserId,
            channel.RemoteEndPoint.Address,
            hwId,
            channel.UserData.ModernHWIds);
    }

    private void ApplyActivePrisonBanRefresh(List<PrisonBanRefreshResult> results)
    {
        foreach (var result in results)
        {
            if (!_player.TryGetSessionById(result.UserId, out var session))
            {
                _prisonUsers.Remove(result.UserId);
                continue;
            }

            if (result.PermanentBan != null)
            {
                ClearPrisonState(session);
                session.Channel.Disconnect(result.PermanentBan.FormatBanMessage(_cfg, _loc));
                continue;
            }

            if (result.TemporaryBan == null)
            {
                ClearPrisonState(session);
                _chat.DispatchServerMessage(session, Loc.GetString("prison-release-message"));
                continue;
            }

            if (!Ready)
            {
                ClearPrisonState(session);
                session.Channel.Disconnect(result.TemporaryBan.FormatBanMessage(_cfg, _loc));
            }
        }
    }

    private void ClearPrisonState(ICommonSession session)
    {
        _prisonUsers.Remove(session.UserId);

        if (session.AttachedEntity is { } entity && Exists(entity))
            RemComp<PrisonBoundComponent>(entity);
    }

    private bool IsUserCurrentlyAntagonist(NetUserId userId)
    {
        return _mind.TryGetMind(userId, out var mindId, out _)
               && _role.MindIsAntagonist(mindId);
    }

    private bool IsSessionAntagonist(ICommonSession session)
    {
        return _mind.TryGetMind(session, out var mindId, out _)
               && _role.MindIsAntagonist(mindId);
    }

    private bool TryGetSpawnCoordinates(out EntityCoordinates coordinates)
    {
        var spawns = new List<EntityCoordinates>();

        var query = EntityQueryEnumerator<PrisonSpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var xform))
        {
            if (xform.MapID == MapId.Nullspace)
                continue;

            spawns.Add(xform.Coordinates);
        }

        if (spawns.Count == 0)
        {
            coordinates = EntityCoordinates.Invalid;
            return false;
        }

        coordinates = _random.Pick(spawns);
        return true;
    }

    private bool IsPrisonMap(MapId mapId)
    {
        var query = EntityQueryEnumerator<PrisonSpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var xform))
        {
            if (xform.MapID == mapId)
                return true;
        }

        return false;
    }

    private void SendPrisonMessage(ICommonSession session, BanDef ban)
    {
        var remaining = ban.ExpirationTime - DateTimeOffset.UtcNow;
        var minutes = remaining is { TotalMinutes: > 0 }
            ? Math.Ceiling(remaining.Value.TotalMinutes).ToString("N0")
            : "0";

        _chat.DispatchServerMessage(session, Loc.GetString("prison-sent-message", ("minutes", minutes)));
    }

    private static bool IsTemporaryServerBan(BanDef ban)
    {
        return ban.Type == BanType.Server
               && ban.Unban == null
               && ban.ExpirationTime is { } expiration
               && expiration > DateTimeOffset.UtcNow;
    }

    private static bool IsPermanentServerBan(BanDef ban)
    {
        return ban.Type == BanType.Server
               && ban.Unban == null
               && ban.ExpirationTime == null;
    }

    private readonly record struct PrisonBanRefreshCheck(
        NetUserId UserId,
        IPAddress Address,
        ImmutableArray<byte>? HwId,
        ImmutableArray<ImmutableArray<byte>> ModernHwIds);

    private readonly record struct PrisonBanRefreshResult(
        NetUserId UserId,
        BanDef? TemporaryBan,
        BanDef? PermanentBan);
}
