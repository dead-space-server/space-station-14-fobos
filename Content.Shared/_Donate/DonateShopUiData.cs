using Robust.Shared.Serialization;

namespace Content.Shared._Donate;

[Serializable, NetSerializable]
public sealed class DonateShopState
{
    public string PlayerUserName { get; set; }
    public string Ss14PlayerId { get; }
    public string OocColor { get; }
    public int ExtraSlots { get; }
    public bool HavePriorityJoinGame { get; }
    public bool HavePriorityAntageGame { get; }
    public bool AllowJob { get; }
    public float Energy { get; }
    public int Crystals { get; }
    public int Level { get; }
    public int Experience { get; }
    public int RequiredExp { get; }
    public int ToNextLevel { get; }
    public float Progress { get; }
    public PremiumData? CurrentPremium { get; }
    public List<DonateItemData> Items { get; }
    public List<DonateSubscribeData> Subscribes { get; }
    public HashSet<string> SpawnedItems { get; set; }

    public DonateShopState(
        string playerUserName,
        string ss14PlayerId,
        string oocColor,
        int extraSlots,
        bool havePriorityJoinGame,
        bool havePriorityAntageGame,
        bool allowJob,
        float energy,
        int crystals,
        int level,
        int experience,
        int requiredExp,
        int toNextLevel,
        float progress,
        PremiumData? currentPremium,
        List<DonateItemData> items,
        List<DonateSubscribeData> subscribes,
        HashSet<string>? spawnedItems = null)
    {
        PlayerUserName = playerUserName;
        Ss14PlayerId = ss14PlayerId;
        OocColor = oocColor;
        ExtraSlots = extraSlots;
        HavePriorityJoinGame = havePriorityJoinGame;
        HavePriorityAntageGame = havePriorityAntageGame;
        AllowJob = allowJob;
        Energy = energy;
        Crystals = crystals;
        Level = level;
        Experience = experience;
        RequiredExp = requiredExp;
        ToNextLevel = toNextLevel;
        Progress = progress;
        CurrentPremium = currentPremium;
        Items = items;
        Subscribes = subscribes;
        SpawnedItems = spawnedItems ?? new HashSet<string>();
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
        int energyPrice = 0)
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
public enum DonateShopUiKey : byte
{
    Key
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
