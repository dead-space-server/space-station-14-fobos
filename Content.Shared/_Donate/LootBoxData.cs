// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared._Donate;

[Serializable, NetSerializable]
public enum LootboxRarity : byte
{
    Common,
    Epic,
    Mythic,
    Legendary
}

[Serializable, NetSerializable]
public enum LootboxRewardType : byte
{
    Item,
    Currency
}

[Serializable, NetSerializable]
public sealed class LootboxItemResult
{
    public int Id { get; }
    public string Name { get; }
    public string? ItemIdInGame { get; }
    public LootboxRarity Rarity { get; }

    public LootboxItemResult(int id, string name, string? itemIdInGame, LootboxRarity rarity)
    {
        Id = id;
        Name = name;
        ItemIdInGame = itemIdInGame;
        Rarity = rarity;
    }
}

[Serializable, NetSerializable]
public sealed class LootboxCurrencyResult
{
    public string CurrencyType { get; }
    public float Amount { get; }

    public LootboxCurrencyResult(string currencyType, float amount)
    {
        CurrencyType = currencyType;
        Amount = amount;
    }
}

[Serializable, NetSerializable]
public sealed class LootboxOpenResult
{
    public bool Success { get; }
    public string Message { get; }
    public string? LootboxName { get; }
    public LootboxRewardType RewardType { get; }
    public LootboxItemResult? Item { get; }
    public LootboxCurrencyResult? Currency { get; }
    public List<LootboxRarity>? Sequence { get; }
    public bool StelsOpen { get; }

    public LootboxOpenResult(
        bool success,
        string message,
        string? lootboxName = null,
        LootboxRewardType rewardType = LootboxRewardType.Item,
        LootboxItemResult? item = null,
        LootboxCurrencyResult? currency = null,
        List<LootboxRarity>? sequence = null,
        bool stelsOpen = false)
    {
        Success = success;
        Message = message;
        LootboxName = lootboxName;
        RewardType = rewardType;
        Item = item;
        Currency = currency;
        Sequence = sequence;
        StelsOpen = stelsOpen;
    }
}

[Serializable, NetSerializable]
public sealed class RequestOpenLootbox : EntityEventArgs
{
    public int UserItemId { get; }
    public bool StelsOpen { get; }

    public RequestOpenLootbox(int userItemId, bool stelsOpen)
    {
        UserItemId = userItemId;
        StelsOpen = stelsOpen;
    }
}

[Serializable, NetSerializable]
public sealed class LootboxOpenedResult : EntityEventArgs
{
    public LootboxOpenResult Result { get; }

    public LootboxOpenedResult(LootboxOpenResult result)
    {
        Result = result;
    }
}
