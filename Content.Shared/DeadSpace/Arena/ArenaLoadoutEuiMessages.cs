using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Arena;

[Serializable, NetSerializable]
public sealed class ArenaLoadoutEuiState : EuiStateBase
{
    public List<ArenaLoadoutOption> Weapons { get; }

    public List<ArenaCostumeOption> Costumes { get; }

    public int Balance { get; }

    public HashSet<int> OwnedCostumes { get; }

    public List<int> EquippedCostumes { get; }

    public List<ArenaTdmListingData> TdmStoreListings { get; }

    public List<string> TdmPurchasedItems { get; }

    public int RemainingBalance { get; }

    public ArenaLoadoutEuiState(
        List<ArenaLoadoutOption> weapons,
        List<ArenaCostumeOption> costumes,
        int balance,
        HashSet<int> ownedCostumes,
        List<int> equippedCostumes,
        List<ArenaTdmListingData> tdmStoreListings,
        List<string> tdmPurchasedItems,
        int remainingBalance)
    {
        Weapons = weapons;
        Costumes = costumes;
        Balance = balance;
        OwnedCostumes = ownedCostumes;
        EquippedCostumes = equippedCostumes;
        TdmStoreListings = tdmStoreListings;
        TdmPurchasedItems = tdmPurchasedItems;
        RemainingBalance = remainingBalance;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaLoadoutOption
{
    public int Index { get; set; }
    public LocId Name { get; set; } = string.Empty;
    public LocId Description { get; set; } = string.Empty;
    public LocId Category { get; set; } = string.Empty;
    public string SpritePrototype { get; set; } = string.Empty;
}

[Serializable, NetSerializable]
public sealed class ArenaLoadoutSelectedMessage : EuiMessageBase
{
    public int WeaponIndex { get; }

    public ArenaLoadoutSelectedMessage(int weaponIndex)
    {
        WeaponIndex = weaponIndex;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaCostumeOption
{
    public int Index { get; set; }
    public string Id { get; set; } = string.Empty;
    public LocId Name { get; set; } = string.Empty;
    public LocId Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ItemPrototype { get; set; } = string.Empty;
    public string Slot { get; set; } = string.Empty;
    public int Price { get; set; }
}

[Serializable, NetSerializable]
public sealed class ArenaCostumeBuyMessage : EuiMessageBase
{
    public int CostumeIndex { get; }

    public ArenaCostumeBuyMessage(int costumeIndex)
    {
        CostumeIndex = costumeIndex;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaCostumeEquipMessage : EuiMessageBase
{
    public List<int> CostumeIndexes { get; }

    public ArenaCostumeEquipMessage(List<int> costumeIndexes)
    {
        CostumeIndexes = costumeIndexes;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaTdmListingData
{
    public string Id { get; set; } = string.Empty;
    public LocId Name { get; set; } = string.Empty;
    public LocId Description { get; set; } = string.Empty;
    public int Cost { get; set; }
    public string SpritePrototype { get; set; } = string.Empty;
    public LocId Category { get; set; } = string.Empty;
}

[Serializable, NetSerializable]
public sealed class ArenaTdmPurchaseConfirmMessage : EuiMessageBase
{
    public List<string> ListingIds { get; }

    public ArenaTdmPurchaseConfirmMessage(List<string> listingIds)
    {
        ListingIds = listingIds;
    }
}
