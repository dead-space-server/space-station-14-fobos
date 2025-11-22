using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Content.Server.GameTicking;
using Content.Shared._Donate;
using Content.Shared.DeadSpace.CCCCVars;
using Content.Shared.GameTicking;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server._Donate;

public sealed class DonateShopSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ISharedPlayerManager _playMan = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    private HttpClient _client = default!;

    private string _apiUrl = string.Empty;
    private string _apiKey = string.Empty;

    private readonly Dictionary<string, DonateShopState> _cache = new();
    private readonly Dictionary<string, HashSet<string>> _spawnedItems = new();

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(CCCCVars.DonateApiUrl, v => _apiUrl = v, true);
        _cfg.OnValueChanged(CCCCVars.DonateApiKey, v => _apiKey = v, true);

        _client = new HttpClient();

        SubscribeNetworkEvent<RequestUpdateDonateShop>(OnUpdate);
        SubscribeNetworkEvent<DonateShopSpawnEvent>(OnSpawnRequest);

        _playMan.PlayerStatusChanged += OnPlayerStatusChanged;

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
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
        if (data != null)
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
            if (data == null)
                return;

            if (_spawnedItems.TryGetValue(userId, out var spawned))
            {
                data.SpawnedItems = spawned;
            }

            _cache[userId] = data;
        }

        if (data.PlayerUserName == "Unknown")
        {
            data.PlayerUserName = args.SenderSession.Name;
        }

        RaiseNetworkEvent(new UpdateDonateShopUIState(data), args.SenderSession.Channel);
    }

    private void OnSpawnRequest(DonateShopSpawnEvent msg, EntitySessionEventArgs args)
    {
        var userId = args.SenderSession.UserId.ToString();

        if (!_cache.TryGetValue(userId, out var state))
            return;

        if (state.SpawnedItems.Contains(msg.ProtoId))
            return;

        if (!_playMan.TryGetSessionById(args.SenderSession.UserId, out var session))
            return;

        if (session.AttachedEntity == null)
            return;

        var playerEntity = session.AttachedEntity.Value;

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

    private async Task<DonateShopState?> FetchDonateData(string userId)
    {
        if (_apiUrl == string.Empty || _apiKey == string.Empty)
            return null;

        try
        {
            var httpClient = _client;
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("API-KEY", _apiKey);

            var response = await httpClient.GetAsync(_apiUrl + userId);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonNode.Parse(json);

            if (data == null)
                return null;

            return ParseDonateData(data);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private DonateShopState? ParseDonateData(JsonNode data)
    {
        try
        {
            var playerUserName = data["player_user_name"]?.GetValue<string>() ?? "Unknown";
            var ss14PlayerId = data["ss14_player_id"]?.GetValue<string>() ?? "";
            var oocColor = data["ooc"]?.GetValue<string>() ?? "#EEEEEE";
            var extraSlots = data["extra_slots"]?.GetValue<int>() ?? 0;
            var havePriorityJoinGame = data["have_priority_join_game"]?.GetValue<bool>() ?? false;
            var havePriorityAntageGame = data["have_priority_antage_game"]?.GetValue<bool>() ?? false;
            var allowJob = data["allow_job"]?.GetValue<bool>() ?? false;
            var energy = data["energy"]?.GetValue<float>() ?? 0f;
            var crystals = 0;
            var level = data["level"]?.GetValue<int>() ?? 1;
            var experience = data["experience"]?.GetValue<int>() ?? 0;
            var requiredExp = data["required_exp"]?.GetValue<int>() ?? 10;
            var toNextLevel = data["to_next_level"]?.GetValue<int>() ?? 10;
            var progress = data["progress"]?.GetValue<float>() ?? 0f;

            var items = new List<DonateItemData>();
            var itemsArray = data["items"]?.AsArray();
            if (itemsArray != null)
            {
                foreach (var itemNode in itemsArray)
                {
                    if (itemNode == null)
                        continue;

                    var itemObj = itemNode["item"];
                    if (itemObj == null)
                        continue;

                    var itemData = new DonateItemData(
                        itemNode["item_id"]?.GetValue<int>() ?? 0,
                        itemObj["name"]?.GetValue<string>() ?? "Unknown",
                        itemObj["item_id_in_game"]?.GetValue<string>(),
                        itemObj["image"]?.GetValue<string>() ?? "",
                        itemObj["category"]?["name"]?.GetValue<string>() ?? "Misc",
                        itemObj["subcategory"]?.GetValue<string>(),
                        itemNode["is_active"]?.GetValue<bool>() ?? false,
                        itemNode["time_allways"]?.GetValue<bool>() ?? false,
                        itemNode["time_start"]?.GetValue<string>(),
                        itemNode["time_finish"]?.GetValue<string>(),
                        itemObj["coin_price"]?.GetValue<int>() ?? 0,
                        itemObj["crystal_price"]?.GetValue<int>() ?? 0,
                        itemObj["energy_price"]?.GetValue<int>() ?? 0
                    );

                    items.Add(itemData);
                }
            }

            var subscribes = new List<DonateSubscribeData>();
            var subscribesArray = data["donate_subscribes"]?.AsArray();
            if (subscribesArray != null)
            {
                foreach (var subNode in subscribesArray)
                {
                    if (subNode == null)
                        continue;

                    var subInfo = subNode["subscribe_info"];
                    if (subInfo == null)
                        continue;

                    var subItems = new List<DonateItemData>();
                    var subItemsArray = subInfo["options"]?["items"]?.AsArray();
                    if (subItemsArray != null)
                    {
                        foreach (var subItemNode in subItemsArray)
                        {
                            if (subItemNode == null)
                                continue;

                            var subItemData = new DonateItemData(
                                subItemNode["id"]?.GetValue<int>() ?? 0,
                                subItemNode["name"]?.GetValue<string>() ?? "Unknown",
                                subItemNode["item_id_in_game"]?.GetValue<string>(),
                                "",
                                "Subscribe Item",
                                null,
                                true,
                                false
                            );

                            subItems.Add(subItemData);
                        }
                    }

                    var subscribeData = new DonateSubscribeData(
                        subInfo["name"]?.GetValue<string>() ?? "Unknown",
                        subInfo["price"]?.GetValue<int>() ?? 0,
                        subInfo["image"]?.GetValue<string>() ?? "",
                        subNode["start_date"]?.GetValue<string>() ?? "",
                        subNode["finish_date"]?.GetValue<string>() ?? "",
                        subItems
                    );

                    subscribes.Add(subscribeData);
                }
            }

            PremiumData? currentPremium = null;
            var premiumNode = data["current_premium"];
            if (premiumNode != null)
            {
                var premiumLevelNode = premiumNode["premium_level"];
                if (premiumLevelNode != null)
                {
                    var premiumLevel = new PremiumLevelData(
                        premiumLevelNode["level"]?.GetValue<int>() ?? 0,
                        premiumLevelNode["name"]?.GetValue<string>() ?? "Unknown",
                        premiumLevelNode["description"]?.GetValue<string>() ?? "",
                        premiumLevelNode["bonus_xp"]?.GetValue<float>() ?? 0f,
                        premiumLevelNode["bonus_energy"]?.GetValue<float>() ?? 0f,
                        premiumLevelNode["bonus_slots"]?.GetValue<int>() ?? 0
                    );

                    currentPremium = new PremiumData(
                        premiumLevel,
                        premiumNode["active"]?.GetValue<bool>() ?? false,
                        premiumNode["expires_in"]?.GetValue<int>() ?? 0
                    );
                }
            }

            return new DonateShopState(
                playerUserName,
                ss14PlayerId,
                oocColor,
                extraSlots,
                havePriorityJoinGame,
                havePriorityAntageGame,
                allowJob,
                energy,
                crystals,
                level,
                experience,
                requiredExp,
                toNextLevel,
                progress,
                currentPremium,
                items,
                subscribes
            );
        }
        catch (Exception)
        {
            return null;
        }
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
