using Content.Shared._Donate;

namespace Content.DeadSpace.Interfaces.Server;

public interface IDonateApiService
{
    Task<DonateShopState?> FetchUserDataAsync(string userId);
    Task<bool> SendUptimeAsync(string userId, DateTime entryTime, DateTime exitTime);
}

