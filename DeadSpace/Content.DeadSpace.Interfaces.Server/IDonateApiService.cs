using Content.Shared._Donate;

namespace Content.DeadSpace.Interfaces.Server;

public interface IDonateApiService
{
    Task<DonateShopState?> FetchUserDataAsync(string userId);
    Task<bool> SendUptimeAsync(string userId, DateTime entryTime, DateTime exitTime);
    void AddSpawnBanTimerForUser(string userId);
    void ClearSpawnBanTimer();
    Task<EnergyShopState> FetchEnergyShopItemsAsync(int page = 1);
    Task<PurchaseResult> PurchaseEnergyItemAsync(int user, int itemId, PurchasePeriod period);
}

