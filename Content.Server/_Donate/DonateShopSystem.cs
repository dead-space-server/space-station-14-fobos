// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using System.Threading.Tasks;
using Content.Server.GameTicking;
using Content.Shared._Donate;
using Content.DeadSpace.Interfaces.Server;
using Content.Shared.GameTicking;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Configuration;
using Content.Shared.DeadSpace.CCCCVars;
using Robust.Shared.Timing;

namespace Content.Server._Donate;

public sealed class DonateShopSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ISharedPlayerManager _playMan = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private readonly ISawmill _sawmill = Logger.GetSawmill("donate.uptime");

    private readonly Dictionary<string, DonateShopState> _cache = new();
    private readonly Dictionary<string, HashSet<string>> _spawnedItems = new();
    private TimeSpan _timeUntilSpawnBan = TimeSpan.Zero;
    private IDonateApiService? _donateApiService;

    private readonly Dictionary<string, DateTime> _playerEntryTimes = new();
    private readonly List<(string UserId, DateTime Entry, DateTime Exit)> _pendingSessions = new();
    private TimeSpan _lastRetryTime = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(CCCCVars.DonateSpawnTimeLimit, v => _timeUntilSpawnBan = _gameTiming.CurTime + TimeSpan.FromMinutes(v), true);

        SubscribeNetworkEvent<RequestUpdateDonateShop>(OnUpdate);
        SubscribeNetworkEvent<DonateShopSpawnEvent>(OnSpawnRequest);

        _playMan.PlayerStatusChanged += OnPlayerStatusChanged;

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        IoCManager.Instance!.TryResolveType(out _donateApiService);

        _sawmill.Info($"DonateShopSystem initialized, API service: {(_donateApiService != null ? "OK" : "NULL")}");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pendingSessions.Count == 0)
            return;

        if (_gameTiming.CurTime - _lastRetryTime < TimeSpan.FromSeconds(60))
            return;

        _lastRetryTime = _gameTiming.CurTime;

        _sawmill.Info($"Retrying {_pendingSessions.Count} pending uptime sessions");

        var toRetry = _pendingSessions.ToList();
        _pendingSessions.Clear();

        foreach (var (userId, entry, exit) in toRetry)
        {
            _ = SendUptimeAsync(userId, entry, exit);
        }
    }

    private async Task SendUptimeAsync(string userId, DateTime entryTime, DateTime exitTime)
    {
        if (_donateApiService == null)
        {
            _sawmill.Warning($"API service is null, queueing for retry: {userId}");
            _pendingSessions.Add((userId, entryTime, exitTime));
            return;
        }

        var duration = (exitTime - entryTime).TotalMinutes;

        var success = await _donateApiService.SendUptimeAsync(userId, entryTime, exitTime);

        if (success)
        {
            _sawmill.Info($"Uptime sent: {userId}, duration: {duration:F1} min");
        }
        else
        {
            _sawmill.Warning($"Uptime send failed, queueing for retry: {userId}, duration: {duration:F1} min");
            _pendingSessions.Add((userId, entryTime, exitTime));
        }
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _cache.Clear();
        _spawnedItems.Clear();
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        var userId = e.Session.UserId.ToString();

        if (e.NewStatus == SessionStatus.Connected)
        {
            _ = FetchAndCachePlayerData(userId);
            _playerEntryTimes[userId] = DateTime.UtcNow;
            _sawmill.Info($"Player connected: {userId}");
        }
        else if (e.NewStatus == SessionStatus.Disconnected)
        {
            _cache.Remove(userId);

            if (_playerEntryTimes.TryGetValue(userId, out var entryTime))
            {
                _playerEntryTimes.Remove(userId);
                var exitTime = DateTime.UtcNow;
                _sawmill.Info($"Player disconnected: {userId}, sending uptime");
                _ = SendUptimeAsync(userId, entryTime, exitTime);
            }
        }
    }

    private async Task FetchAndCachePlayerData(string userId)
    {
        var data = await FetchDonateData(userId);
        if (data.IsRegistered != false)
        {
            if (_spawnedItems.TryGetValue(userId, out var spawned))
            {
                data.SpawnedItems = spawned;
            }
            _cache[userId] = data;
        }
    }

    private void OnUpdate(RequestUpdateDonateShop msg, EntitySessionEventArgs args)
    {
        _ = PrepareUpdate(args);
    }

    private async Task PrepareUpdate(EntitySessionEventArgs args)
    {
        var userId = args.SenderSession.UserId.ToString();

        if (!_cache.TryGetValue(userId, out var data))
        {
            data = await FetchDonateData(userId);

            if (data.IsRegistered != false)
            {
                if (_spawnedItems.TryGetValue(userId, out var spawned))
                    data.SpawnedItems = spawned;

                _cache[userId] = data;
            }
        }

        if (data.PlayerUserName == "Unknown")
        {
            data.PlayerUserName = args.SenderSession.Name;
        }

        RaiseNetworkEvent(new UpdateDonateShopUIState(data), args.SenderSession.Channel);
    }

    private void OnSpawnRequest(DonateShopSpawnEvent msg, EntitySessionEventArgs args)
    {
        if (_gameTiming.CurTime > _timeUntilSpawnBan)
            return;

        var userId = args.SenderSession.UserId.ToString();

        if (!_cache.TryGetValue(userId, out var state))
            return;

        if (state.SpawnedItems.Contains(msg.ProtoId))
            return;

        if (args.SenderSession.AttachedEntity == null)
            return;

        var playerEntity = args.SenderSession.AttachedEntity.Value;

        if (!HasComp<HumanoidAppearanceComponent>(playerEntity) || !_mobState.IsAlive(playerEntity))
            return;

        var allItems = new List<DonateItemData>(state.Items);
        foreach (var sub in state.Subscribes)
        {
            foreach (var subItem in sub.Items)
            {
                if (allItems.All(i => i.ItemIdInGame != subItem.ItemIdInGame))
                {
                    allItems.Add(subItem);
                }
            }
        }

        var item = allItems.FirstOrDefault(i => i.ItemIdInGame == msg.ProtoId);
        if (item == null || !item.IsActive)
            return;

        if (_gameTicker.RunLevel != GameRunLevel.InRound)
            return;

        var playerTransform = Transform(playerEntity);
        var spawnedEntity = Spawn(msg.ProtoId, _transform.GetMapCoordinates(playerTransform));
        _handsSystem.TryPickupAnyHand(playerEntity, spawnedEntity);

        if (!_spawnedItems.ContainsKey(userId))
        {
            _spawnedItems[userId] = new HashSet<string>();
        }

        _spawnedItems[userId].Add(msg.ProtoId);
        state.SpawnedItems.Add(msg.ProtoId);

        RaiseNetworkEvent(new UpdateDonateShopUIState(state), args.SenderSession.Channel);
    }

    private async Task<DonateShopState> FetchDonateData(string userId)
    {
        if (_donateApiService == null)
            return new DonateShopState("Веб сервис не доступен.");

        var apiResponse = await _donateApiService!.FetchUserDataAsync(userId);

        if (apiResponse == null)
            return new DonateShopState("Ошибка при загрузке данных");

        return apiResponse;
    }

    public async Task RefreshPlayerCache(string userId)
    {
        await FetchAndCachePlayerData(userId);
    }

    public DonateShopState? GetCachedData(string userId)
    {
        return _cache.TryGetValue(userId, out var data) ? data : null;
    }
}
