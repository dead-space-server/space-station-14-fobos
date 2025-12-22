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
    public int Crystals { get; } = 0;
    public int Level { get; } = 1;
    public int Experience { get; } = 0;
    public int RequiredExp { get; } = 10;
    public int ToNextLevel { get; } = 10;
    public float Progress { get; } = 0f;
    public int User { get; } = 0;
    public PremiumData? CurrentPremium { get; }
    public List<DonateItemData> Items { get; } = new List<DonateItemData>();
    public List<DonateSubscribeData> Subscribes { get; } = new List<DonateSubscribeData>();
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
        int crystals,
        int level,
        int experience,
        int requiredExp,
        int toNextLevel,
        float progress,
        int user,
        PremiumData? currentPremium,
        List<DonateItemData> items,
        List<DonateSubscribeData> subscribes,
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
        Items = items;
        Subscribes = subscribes;
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
public sealed class DonateItemData
{
    public int ItemId { get; }
    public string ItemName { get; }
    public string? ItemIdInGame { get; }
    public string ImageUrl { get; }
    public string Category { get; }
    public string? Subcategory { get; }
    public bool IsActive { get; }
    public bool TimeAllways { get; }
    public string? TimeStart { get; }
    public string? TimeFinish { get; }
    public int CoinPrice { get; }
    public int CrystalPrice { get; }
    public int EnergyPrice { get; }
    public string? SourceSubscription { get; }

    public DonateItemData(
        int itemId,
        string itemName,
        string? itemIdInGame,
        string imageUrl,
        string category,
        string? subcategory,
        bool isActive,
        bool timeAllways,
        string? timeStart = null,
        string? timeFinish = null,
        int coinPrice = 0,
        int crystalPrice = 0,
        int energyPrice = 0,
        string? sourceSubscription = null)
    {
        ItemId = itemId;
        ItemName = itemName;
        ItemIdInGame = itemIdInGame;
        ImageUrl = imageUrl;
        Category = category;
        Subcategory = subcategory;
        IsActive = isActive;
        TimeAllways = timeAllways;
        TimeStart = timeStart;
        TimeFinish = timeFinish;
        CoinPrice = coinPrice;
        CrystalPrice = crystalPrice;
        EnergyPrice = energyPrice;
        SourceSubscription = sourceSubscription;
    }
}

[Serializable, NetSerializable]
public sealed class DonateSubscribeData
{
    public string SubscribeName { get; }
    public int Price { get; }
    public string ImageUrl { get; }
    public string StartDate { get; }
    public string FinishDate { get; }
    public List<DonateItemData> Items { get; }

    public DonateSubscribeData(
        string subscribeName,
        int price,
        string imageUrl,
        string startDate,
        string finishDate,
        List<DonateItemData> items)
    {
        SubscribeName = subscribeName;
        Price = price;
        ImageUrl = imageUrl;
        StartDate = startDate;
        FinishDate = finishDate;
        Items = items;
    }
}

[Serializable, NetSerializable]
public sealed class PremiumData
{
    public PremiumLevelData PremiumLevel { get; }
    public bool Active { get; }
    public int ExpiresIn { get; }

    public PremiumData(
        PremiumLevelData premiumLevel,
        bool active,
        int expiresIn)
    {
        PremiumLevel = premiumLevel;
        Active = active;
        ExpiresIn = expiresIn;
    }
}

[Serializable, NetSerializable]
public sealed class PremiumLevelData
{
    public int Level { get; }
    public string Name { get; }
    public string Description { get; }
    public float BonusXp { get; }
    public float BonusEnergy { get; }
    public int BonusSlots { get; }

    public PremiumLevelData(
        int level,
        string name,
        string description,
        float bonusXp,
        float bonusEnergy,
        int bonusSlots)
    {
        Level = level;
        Name = name;
        Description = description;
        BonusXp = bonusXp;
        BonusEnergy = bonusEnergy;
        BonusSlots = bonusSlots;
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
public sealed class RequestUpdateDonateShop : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class UpdateDonateShopUIState(DonateShopState state) : EntityEventArgs
{
    public DonateShopState State = state;
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
