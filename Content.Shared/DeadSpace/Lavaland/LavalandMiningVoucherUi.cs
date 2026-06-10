using Content.Shared.DeadSpace.Lavaland.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Lavaland;

[Serializable, NetSerializable]
public enum LavalandMiningVoucherUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class LavalandMiningVoucherEntry
{
    public int Index { get; init; }
    public string Name { get; init; } = string.Empty;
    public EntProtoId? IconPrototype { get; init; }
    public List<LavalandMiningVoucherItemEntry> Contents { get; init; } = new();
}

[Serializable, NetSerializable]
public sealed class LavalandMiningVoucherItemEntry
{
    public EntProtoId Prototype { get; init; }
    public int Amount { get; init; }
}

[Serializable, NetSerializable]
public sealed class LavalandMiningVoucherBoundUserInterfaceState : BoundUserInterfaceState
{
    public List<LavalandMiningVoucherEntry> Rewards { get; init; } = new();
    public bool Powered { get; init; }
}

[Serializable, NetSerializable]
public sealed class LavalandMiningVoucherRedeemMessage : BoundUserInterfaceMessage
{
    public int Index { get; init; }
}

[Serializable, NetSerializable]
public sealed class LavalandMiningVoucherEjectMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class LavalandMiningVoucherRequestUpdateMessage : BoundUserInterfaceMessage;

public static class LavalandMiningVoucherUi
{
    public static LavalandMiningVoucherBoundUserInterfaceState CreateState(
        LavalandMiningVoucherComponent? voucher,
        bool powered)
    {
        var rewards = new List<LavalandMiningVoucherEntry>();
        if (voucher == null)
            return new LavalandMiningVoucherBoundUserInterfaceState { Rewards = rewards, Powered = powered };

        for (var i = 0; i < voucher.Rewards.Count; i++)
        {
            var reward = voucher.Rewards[i];
            var contents = new List<LavalandMiningVoucherItemEntry>();
            EntProtoId? iconPrototype = null;

            foreach (var entry in reward.Entries)
            {
                if (entry.PrototypeId is not { } prototype)
                    continue;

                iconPrototype ??= prototype;
                contents.Add(new LavalandMiningVoucherItemEntry
                {
                    Prototype = prototype,
                    Amount = Math.Max(1, entry.Amount),
                });
            }

            rewards.Add(new LavalandMiningVoucherEntry
            {
                Index = i,
                Name = Loc.GetString(reward.Name),
                IconPrototype = iconPrototype,
                Contents = contents,
            });
        }

        return new LavalandMiningVoucherBoundUserInterfaceState
        {
            Rewards = rewards,
            Powered = powered,
        };
    }
}
