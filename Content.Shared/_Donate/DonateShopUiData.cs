// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared._Donate;

[Serializable, NetSerializable]
public enum DonateShopUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum PurchasePeriod : byte
{
    Week,
    Month,
    ThreeMonth,
    SixMonth,
    Year,
    Always
}

[Serializable, NetSerializable]
public enum CalendarRewardStatus : byte
{
    Locked,
    Available,
    Claimed,
    Missed
}

[Serializable, NetSerializable]
public sealed class DonateShopState
{
    public string PlayerUserName { get; set; } = "Unknown";
    public string Ss14PlayerId { get; } = string.Empty;
    public string OocColor { get; } = "#EEEEEE";
    public string ErrorMessage { get; } = string.Empty;
    public int ExtraSlots { get; } = 0;
    public bool IsRegistered { get; } = false;
    public bool HasError { get; } = false;
    public bool HavePriorityJoinGame { get; } = false;
    public bool HavePriorityAntageGame { get; } = false;
    public bool AllowJob { get; } = false;
    public bool IsTimeUp { get; } = false;
    public float Energy { get; } = 0f;
    public float Crystals { get; } = 0f;
    public int Level { get; } = 1;
    public int Experience { get; } = 0;
    public int RequiredExp { get; } = 10;
    public int ToNextLevel { get; } = 10;
    public float Progress { get; } = 0f;
    public int User { get; } = 0;
    public PremiumData? CurrentPremium { get; }
    public ActiveSubscriptionData? ActiveSubscription { get; }
    public AdminRankData? AdminRank { get; }
    public HashSet<string> SpawnedItems { get; set; } = new HashSet<string>();

    public DonateShopState(
        string playerUserName,
        string ss14PlayerId,
        string oocColor,
        string errorMessage,
        int extraSlots,
        bool isRegistered,
        bool havePriorityJoinGame,
        bool havePriorityAntageGame,
        bool allowJob,
        bool hasError,
        bool isTimeUp,
        float energy,
        float crystals,
        int level,
        int experience,
        int requiredExp,
        int toNextLevel,
        float progress,
        int user,
        PremiumData? currentPremium,
        ActiveSubscriptionData? activeSubscription,
        AdminRankData? adminRank = null,
        HashSet<string>? spawnedItems = null)
    {
        PlayerUserName = playerUserName;
        Ss14PlayerId = ss14PlayerId;
        OocColor = oocColor;
        ErrorMessage = errorMessage;
        ExtraSlots = extraSlots;
        IsRegistered = isRegistered;
        HavePriorityJoinGame = havePriorityJoinGame;
        HavePriorityAntageGame = havePriorityAntageGame;
        AllowJob = allowJob;
        HasError = hasError;
        IsTimeUp = isTimeUp;
        Energy = energy;
        Crystals = crystals;
        Level = level;
        Experience = experience;
        RequiredExp = requiredExp;
        ToNextLevel = toNextLevel;
        Progress = progress;
        User = user;
        CurrentPremium = currentPremium;
        ActiveSubscription = activeSubscription;
        AdminRank = adminRank;
        SpawnedItems = spawnedItems ?? new HashSet<string>();
    }

    public DonateShopState(bool isRegistered)
    {
        IsRegistered = isRegistered;
    }

    public DonateShopState(string errorMessage)
    {
        ErrorMessage = errorMessage;
        HasError = true;
    }
}

[Serializable, NetSerializable]
public sealed class ActiveSubscriptionData
{
    public string Name { get; }
    public string FinishDate { get; }
    public string StartDate { get; }
    public string Description { get; }
    public string OocColor { get; }
    public int ExtraSlots { get; }
    public bool HavePriorityJoinGame { get; }
    public bool HavePriorityAntageGame { get; }
    public bool AllowJob { get; }

    public ActiveSubscriptionData(
        string name,
        string finishDate,
        string startDate,
        string description,
        string oocColor,
        int extraSlots,
        bool havePriorityJoinGame,
        bool havePriorityAntageGame,
        bool allowJob)
    {
        Name = name;
        FinishDate = finishDate;
        StartDate = startDate;
        Description = description;
        OocColor = oocColor;
        ExtraSlots = extraSlots;
        HavePriorityJoinGame = havePriorityJoinGame;
        HavePriorityAntageGame = havePriorityAntageGame;
        AllowJob = allowJob;
    }
}

[Serializable, NetSerializable]
public sealed class AdminRankData
{
    public string Name { get; }
    public string? OocColor { get; }
    public int ExtraSlots { get; }
    public bool HavePriorityJoinGame { get; }
    public bool HavePriorityAntageGame { get; }
    public bool AllowJob { get; }

    public AdminRankData(
        string name,
        string? oocColor,
        int extraSlots,
        bool havePriorityJoinGame,
        bool havePriorityAntageGame,
        bool allowJob)
    {
        Name = name;
        OocColor = oocColor;
        ExtraSlots = extraSlots;
        HavePriorityJoinGame = havePriorityJoinGame;
        HavePriorityAntageGame = havePriorityAntageGame;
        AllowJob = allowJob;
    }
}

[Serializable, NetSerializable]
public sealed class InventoryState
{
    public int UserId { get; }
    public string? PlayerUserName { get; }
    public string Ss14PlayerId { get; }
    public int TotalItems { get; }
    public List<InventoryItemData> Items { get; }
    public bool HasError { get; }
    public string ErrorMessage { get; }

    public InventoryState(
        int userId,
        string? playerUserName,
        string ss14PlayerId,
        int totalItems,
        List<InventoryItemData> items)
    {
        UserId = userId;
        PlayerUserName = playerUserName;
        Ss14PlayerId = ss14PlayerId;
        TotalItems = totalItems;
        Items = items;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    public InventoryState(string errorMessage)
    {
        UserId = 0;
        PlayerUserName = null;
        Ss14PlayerId = string.Empty;
        TotalItems = 0;
        Items = new List<InventoryItemData>();
        HasError = true;
        ErrorMessage = errorMessage;
    }
}

[Serializable, NetSerializable]
public sealed class InventoryItemData
{
    public int Id { get; }
    public string Name { get; }
    public string? ItemIdInGame { get; }
    public string Category { get; }
    public string Source { get; }
    public string? TimeFinish { get; }
    public bool TimeAllways { get; }
    public bool IsLootbox { get; }
    public bool StelsHidden { get; }
    public int UserItemId { get; }

    public InventoryItemData(
        int id,
        string name,
        string? itemIdInGame,
        string category,
        string source,
        string? timeFinish,
        bool timeAllways,
        bool isLootbox,
        bool stelsHidden,
        int userItemId)
    {
        Id = id;
        Name = name;
        ItemIdInGame = itemIdInGame;
        Category = category;
        Source = source;
        TimeFinish = timeFinish;
        TimeAllways = timeAllways;
        IsLootbox = isLootbox;
        StelsHidden = stelsHidden;
        UserItemId = userItemId;
    }
}

[Serializable, NetSerializable]
public sealed class PremiumData
{
    public PremiumLevelData PremiumLevel { get; }
    public bool Active { get; }
    public int ExpiresIn { get; }
    public string StartedAt { get; }
    public string ExpiresAt { get; }

    public PremiumData(
        PremiumLevelData premiumLevel,
        bool active,
        int expiresIn,
        string startedAt = "",
        string expiresAt = "")
    {
        PremiumLevel = premiumLevel;
        Active = active;
        ExpiresIn = expiresIn;
        StartedAt = startedAt;
        ExpiresAt = expiresAt;
    }
}

[Serializable, NetSerializable]
public sealed class PremiumLevelData
{
    public int Id { get; }
    public int Level { get; }
    public string Name { get; }
    public string Description { get; }
    public int DurationDays { get; }
    public float BonusXp { get; }
    public float BonusEnergy { get; }
    public int BonusSlots { get; }
    public string Price { get; }
    public bool HavePriorityJoinGame { get; }
    public bool HavePriorityAntageGame { get; }
    public bool AllowJob { get; }

    public PremiumLevelData(
        int id,
        int level,
        string name,
        string description,
        int durationDays,
        float bonusXp,
        float bonusEnergy,
        int bonusSlots,
        string price = "0",
        bool havePriorityJoinGame = false,
        bool havePriorityAntageGame = false,
        bool allowJob = false)
    {
        Id = id;
        Level = level;
        Name = name;
        Description = description;
        DurationDays = durationDays;
        BonusXp = bonusXp;
        BonusEnergy = bonusEnergy;
        BonusSlots = bonusSlots;
        Price = price;
        HavePriorityJoinGame = havePriorityJoinGame;
        HavePriorityAntageGame = havePriorityAntageGame;
        AllowJob = allowJob;
    }
}

[Serializable, NetSerializable]
public sealed class EnergyShopItemData
{
    public int Id { get; }
    public string Name { get; }
    public string ItemIdInGame { get; }
    public string ImageUrl { get; }
    public string Category { get; }
    public string? Subcategory { get; }
    public Dictionary<PurchasePeriod, float> Prices { get; }
    public bool Owned { get; }

    public EnergyShopItemData(
        int id,
        string name,
        string itemIdInGame,
        string imageUrl,
        string category,
        string? subcategory,
        Dictionary<PurchasePeriod, float> prices,
        bool owned)
    {
        Id = id;
        Name = name;
        ItemIdInGame = itemIdInGame;
        ImageUrl = imageUrl;
        Category = category;
        Subcategory = subcategory;
        Prices = prices;
        Owned = owned;
    }
}

[Serializable, NetSerializable]
public sealed class EnergyShopState
{
    public List<EnergyShopItemData> Items { get; }
    public bool HasError { get; }
    public string ErrorMessage { get; }
    public int TotalCount { get; }
    public bool HasNextPage { get; }

    public EnergyShopState(List<EnergyShopItemData> items, int totalCount, bool hasNextPage)
    {
        Items = items;
        TotalCount = totalCount;
        HasNextPage = hasNextPage;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    public EnergyShopState(string errorMessage)
    {
        Items = new List<EnergyShopItemData>();
        HasError = true;
        ErrorMessage = errorMessage;
        TotalCount = 0;
        HasNextPage = false;
    }
}

[Serializable, NetSerializable]
public sealed class PurchaseResult
{
    public bool Success { get; }
    public string Message { get; }

    public PurchaseResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

[Serializable, NetSerializable]
public sealed class CalendarRewardItemData
{
    public int Id { get; }
    public string Name { get; }
    public string? ItemIdInGame { get; }
    public bool Owned { get; }
    public int RewardId { get; }
    public bool IsHidden { get; }
    public bool IsLootbox { get; }

    public CalendarRewardItemData(int id, string name, string? itemIdInGame, bool owned, int rewardId = 0, bool isHidden = false, bool isLootbox = false)
    {
        Id = id;
        Name = name;
        ItemIdInGame = itemIdInGame;
        Owned = owned;
        RewardId = rewardId;
        IsHidden = isHidden;
        IsLootbox = isLootbox;
    }
}

[Serializable, NetSerializable]
public sealed class CalendarDayReward
{
    public int RewardId { get; }
    public int Day { get; }
    public CalendarRewardStatus Status { get; }
    public CalendarRewardItemData Item { get; }

    public CalendarDayReward(int rewardId, int day, CalendarRewardStatus status, CalendarRewardItemData item)
    {
        RewardId = rewardId;
        Day = day;
        Status = status;
        Item = item;
    }
}

[Serializable, NetSerializable]
public sealed class CalendarPreviewDay
{
    public CalendarRewardItemData? Item { get; }
    public CalendarRewardStatus Status { get; }

    public CalendarPreviewDay(CalendarRewardItemData? item, CalendarRewardStatus status)
    {
        Item = item;
        Status = status;
    }
}

[Serializable, NetSerializable]
public sealed class CalendarPreview
{
    public CalendarPreviewDay? Yesterday { get; }
    public CalendarPreviewDay? Today { get; }
    public CalendarPreviewDay? Tomorrow { get; }

    public CalendarPreview(CalendarPreviewDay? yesterday, CalendarPreviewDay? today, CalendarPreviewDay? tomorrow)
    {
        Yesterday = yesterday;
        Today = today;
        Tomorrow = tomorrow;
    }
}

[Serializable, NetSerializable]
public sealed class CalendarProgress
{
    public int CurrentDay { get; }
    public int PoolId { get; }

    public CalendarProgress(int currentDay, int poolId)
    {
        CurrentDay = currentDay;
        PoolId = poolId;
    }
}

[Serializable, NetSerializable]
public sealed class DailyCalendarState
{
    public string CalendarName { get; }
    public List<CalendarDayReward> NormalRewards { get; }
    public List<CalendarDayReward> PremiumRewards { get; }
    public CalendarPreview? NormalPreview { get; }
    public CalendarPreview? PremiumPreview { get; }
    public CalendarProgress? Progress { get; }
    public bool HasError { get; }
    public string ErrorMessage { get; }

    public DailyCalendarState(
        string calendarName,
        List<CalendarDayReward> normalRewards,
        List<CalendarDayReward> premiumRewards,
        CalendarPreview? normalPreview,
        CalendarPreview? premiumPreview,
        CalendarProgress? progress)
    {
        CalendarName = calendarName;
        NormalRewards = normalRewards;
        PremiumRewards = premiumRewards;
        NormalPreview = normalPreview;
        PremiumPreview = premiumPreview;
        Progress = progress;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    public DailyCalendarState(string errorMessage)
    {
        CalendarName = string.Empty;
        NormalRewards = new List<CalendarDayReward>();
        PremiumRewards = new List<CalendarDayReward>();
        HasError = true;
        ErrorMessage = errorMessage;
    }
}

[Serializable, NetSerializable]
public sealed class ClaimRewardResult
{
    public bool Success { get; }
    public string Message { get; }
    public CalendarRewardItemData? ClaimedItem { get; }
    public bool IsPremium { get; }

    public ClaimRewardResult(bool success, string message, CalendarRewardItemData? claimedItem = null, bool isPremium = false)
    {
        Success = success;
        Message = message;
        ClaimedItem = claimedItem;
        IsPremium = isPremium;
    }
}

[Serializable, NetSerializable]
public sealed class RequestUpdateDonateShop : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class UpdateDonateShopUIState : EntityEventArgs
{
    public DonateShopState State { get; }

    public UpdateDonateShopUIState(DonateShopState state)
    {
        State = state;
    }
}

[Serializable, NetSerializable]
public sealed class RequestInventory : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class UpdateInventoryState : EntityEventArgs
{
    public InventoryState State { get; }

    public UpdateInventoryState(InventoryState state)
    {
        State = state;
    }
}

[Serializable, NetSerializable]
public sealed class DonateShopSpawnEvent : EntityEventArgs
{
    public string ProtoId { get; }

    public DonateShopSpawnEvent(string protoId)
    {
        ProtoId = protoId;
    }
}

[Serializable, NetSerializable]
public sealed class RequestEnergyShopItems : EntityEventArgs
{
    public int Page { get; }

    public RequestEnergyShopItems(int page = 1)
    {
        Page = page;
    }
}

[Serializable, NetSerializable]
public sealed class UpdateEnergyShopState : EntityEventArgs
{
    public EnergyShopState State { get; }

    public UpdateEnergyShopState(EnergyShopState state)
    {
        State = state;
    }
}

[Serializable, NetSerializable]
public sealed class RequestPurchaseEnergyItem : EntityEventArgs
{
    public int ItemId { get; }
    public PurchasePeriod Period { get; }

    public RequestPurchaseEnergyItem(int itemId, PurchasePeriod period)
    {
        ItemId = itemId;
        Period = period;
    }
}

[Serializable, NetSerializable]
public sealed class PurchaseEnergyItemResult : EntityEventArgs
{
    public PurchaseResult Result { get; }

    public PurchaseEnergyItemResult(PurchaseResult result)
    {
        Result = result;
    }
}

[Serializable, NetSerializable]
public sealed class RequestDailyCalendar : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class UpdateDailyCalendarState : EntityEventArgs
{
    public DailyCalendarState State { get; }

    public UpdateDailyCalendarState(DailyCalendarState state)
    {
        State = state;
    }
}

[Serializable, NetSerializable]
public sealed class RequestClaimCalendarReward : EntityEventArgs
{
    public int RewardId { get; }
    public bool IsPremium { get; }

    public RequestClaimCalendarReward(int rewardId, bool isPremium)
    {
        RewardId = rewardId;
        IsPremium = isPremium;
    }
}

[Serializable, NetSerializable]
public sealed class ClaimCalendarRewardResult : EntityEventArgs
{
    public ClaimRewardResult Result { get; }

    public ClaimCalendarRewardResult(ClaimRewardResult result)
    {
        Result = result;
    }
}
