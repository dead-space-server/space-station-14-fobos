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
    private readonly Dictionary<string, DonateShopState> _cache = new();
    private readonly Dictionary<string, HashSet<string>> _spawnedItems = new();
    private TimeSpan _timeUntilSpawnBan = TimeSpan.Zero;
    private IDonateApiService? _donateApiService;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(CCCCVars.DonateSpawnTimeLimit, v => _timeUntilSpawnBan = _gameTiming.CurTime + TimeSpan.FromMinutes(v), true);

        SubscribeNetworkEvent<RequestUpdateDonateShop>(OnUpdate);
        SubscribeNetworkEvent<DonateShopSpawnEvent>(OnSpawnRequest);

        _playMan.PlayerStatusChanged += OnPlayerStatusChanged;

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        IoCManager.Instance!.TryResolveType(out _donateApiService);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _cache.Clear();
        _spawnedItems.Clear();
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Connected)
        {
            _ = FetchAndCachePlayerData(e.Session.UserId.ToString());
        }
        else if (e.NewStatus == SessionStatus.Disconnected)
        {
            _cache.Remove(e.Session.UserId.ToString());
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
