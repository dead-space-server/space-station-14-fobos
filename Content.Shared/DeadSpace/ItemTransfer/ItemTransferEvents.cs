using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.ItemTransfer;

[Serializable, NetSerializable]
public sealed class ItemTransferOfferMessage : EntityEventArgs
{
    public readonly int RequestId;
    public readonly string UserName;
    public readonly string ItemName;

    public ItemTransferOfferMessage(int requestId, string userName, string itemName)
    {
        RequestId = requestId;
        UserName = userName;
        ItemName = itemName;
    }
}

[Serializable, NetSerializable]
public sealed class ItemTransferAnswerMessage : EntityEventArgs
{
    public readonly int RequestId;
    public readonly bool Accepted;

    public ItemTransferAnswerMessage(int requestId, bool accepted)
    {
        RequestId = requestId;
        Accepted = accepted;
    }
}

[Serializable, NetSerializable]
public sealed class ItemTransferOfferClosedMessage : EntityEventArgs
{
    public readonly int RequestId;

    public ItemTransferOfferClosedMessage(int requestId)
    {
        RequestId = requestId;
    }
}
