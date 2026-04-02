using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NewsReaderUiMessageEvent : CartridgeMessageEvent
{
    public readonly NewsReaderUiAction Action;
    public readonly string? CommentContent;

    public NewsReaderUiMessageEvent(NewsReaderUiAction action, string? commentContent = null)
    {
        Action = action;
        CommentContent = commentContent;
    }
}

[Serializable, NetSerializable]
public enum NewsReaderUiAction
{
    Next,
    Prev,
    NotificationSwitch,
    Like,
    Dislike,
    AddComment
}
