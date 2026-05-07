using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NewsReaderUiMessageEvent : CartridgeMessageEvent
{
    public readonly NewsReaderUiAction Action;

    // DS14
    public readonly string? CommentContent;

    // DS14
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
    // DS14
    Like,
    // DS14
    Dislike,
    // DS14
    AddComment
}
