// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared._Donate;

namespace Content.DeadSpace.Interfaces.Server;

public enum UptimeResult
{
    Success,
    NotFound,
    NeedsRetry,
}

public interface IDonateApiService
{
    Task<DonateShopState?> FetchUserDataAsync(string userId);
    Task<InventoryState?> FetchInventoryAsync(string userId);
    Task<UptimeResult> SendUptimeAsync(string userId, DateTime entryTime, DateTime exitTime);
    void AddSpawnBanTimerForUser(string userId);
    void ClearSpawnBanTimer();
    Task<EnergyShopState> FetchEnergyShopItemsAsync(int page = 1);
    Task<PurchaseResult> PurchaseEnergyItemAsync(int user, int itemId, PurchasePeriod period);
    Task<DailyCalendarState> FetchDailyCalendarAsync(string userId);
    Task<ClaimRewardResult> ClaimCalendarRewardAsync(string userId, int rewardId);
    Task<LootboxOpenResult> OpenLootboxAsync(string userId, int userItemId, bool stelsOpen);
}

