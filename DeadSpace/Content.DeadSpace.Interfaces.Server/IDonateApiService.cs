using Content.Shared._Donate;

namespace Content.DeadSpace.Interfaces.Server;

public interface IDonateApiService
{
    Task<DonateShopState?> FetchUserDataAsync(string userId);
}

