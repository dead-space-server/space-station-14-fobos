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
using Robust.Shared.Timing;
using Content.Shared.Roles;

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
    [Dependency] private readonly ActorSystem _actorSystem = default!;

    private readonly ISawmill _sawmill = Logger.GetSawmill("donate.uptime");

    private readonly Dictionary<string, DonateShopState> _cache = new();
    private readonly Dictionary<string, HashSet<string>> _spawnedItems = new();
    private IDonateApiService? _donateApiService;

    private readonly Dictionary<string, DateTime> _playerEntryTimes = new();
    private readonly List<(string UserId, DateTime Entry, DateTime Exit)> _pendingSessions = new();
    private TimeSpan _lastRetryTime = TimeSpan.Zero;

    private EnergyShopState? _energyShopCache;
    private TimeSpan _energyShopCacheTime = TimeSpan.Zero;
    private static readonly TimeSpan EnergyShopCacheDuration = TimeSpan.FromMinutes(5);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RequestUpdateDonateShop>(OnUpdate);
        SubscribeNetworkEvent<DonateShopSpawnEvent>(OnSpawnRequest);
        SubscribeNetworkEvent<RequestEnergyShopItems>(OnRequestEnergyShop);
        SubscribeNetworkEvent<RequestPurchaseEnergyItem>(OnPurchaseEnergyItem);

        _playMan.PlayerStatusChanged += OnPlayerStatusChanged;

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<StartingGearEquippedEvent>(OnStartingGearEquipped);

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

    private void OnStartingGearEquipped(ref StartingGearEquippedEvent ev)
    {
        if (_donateApiService != null && _actorSystem.TryGetSession(ev.Entity, out var session) && session != null)
            _donateApiService.AddSpawnBanTimerForUser(session.UserId.ToString());
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

        if (_donateApiService != null)
            _donateApiService.ClearSpawnBanTimer();
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

    private void OnRequestEnergyShop(RequestEnergyShopItems msg, EntitySessionEventArgs args)
    {
        _ = PrepareEnergyShopUpdate(msg, args);
    }

    private async Task PrepareEnergyShopUpdate(RequestEnergyShopItems msg, EntitySessionEventArgs args)
    {
        if (_donateApiService == null)
        {
            RaiseNetworkEvent(new UpdateEnergyShopState(new EnergyShopState("Сервис недоступен")), args.SenderSession.Channel);
            return;
        }

        if (msg.Page == 1 && _energyShopCache != null && _gameTiming.CurTime - _energyShopCacheTime < EnergyShopCacheDuration)
        {
            RaiseNetworkEvent(new UpdateEnergyShopState(_energyShopCache), args.SenderSession.Channel);
            return;
        }

        var state = await _donateApiService.FetchEnergyShopItemsAsync(msg.Page);

        if (msg.Page == 1 && !state.HasError)
        {
            _energyShopCache = state;
            _energyShopCacheTime = _gameTiming.CurTime;
        }

        RaiseNetworkEvent(new UpdateEnergyShopState(state), args.SenderSession.Channel);
    }

    private void OnPurchaseEnergyItem(RequestPurchaseEnergyItem msg, EntitySessionEventArgs args)
    {
        _ = ProcessPurchase(msg, args);
    }

    private async Task ProcessPurchase(RequestPurchaseEnergyItem msg, EntitySessionEventArgs args)
    {
        var sessionUserId = args.SenderSession.UserId.ToString();

        if (_donateApiService == null)
        {
            RaiseNetworkEvent(new PurchaseEnergyItemResult(new PurchaseResult(false, "Сервис недоступен")), args.SenderSession.Channel);
            return;
        }

        if (!_cache.TryGetValue(sessionUserId, out var cachedData) || cachedData.User == 0)
        {
            RaiseNetworkEvent(new PurchaseEnergyItemResult(new PurchaseResult(false, "Данные пользователя не загружены")), args.SenderSession.Channel);
            return;
        }

        var result = await _donateApiService.PurchaseEnergyItemAsync(cachedData.User, msg.ItemId, msg.Period);

        RaiseNetworkEvent(new PurchaseEnergyItemResult(result), args.SenderSession.Channel);

        if (result.Success)
        {
            _cache.Remove(sessionUserId);
            await FetchAndCachePlayerData(sessionUserId);

            if (_cache.TryGetValue(sessionUserId, out var newData))
            {
                RaiseNetworkEvent(new UpdateDonateShopUIState(newData), args.SenderSession.Channel);
            }

            _energyShopCache = null;
        }
    }

    private void OnSpawnRequest(DonateShopSpawnEvent msg, EntitySessionEventArgs args)
    {
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
            return new DonateShopState("Ведутся технические работы, сервис будет доступен позже.");

        var apiResponse = await _donateApiService!.FetchUserDataAsync(userId);

        if (apiResponse == null)
            return new DonateShopState("Ведутся технические работы, сервис будет доступен позже.");

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
