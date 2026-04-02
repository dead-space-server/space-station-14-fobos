using Robust.Shared.Serialization;

namespace Content.Shared.MassMedia.Systems;

public abstract class SharedNewsSystem : EntitySystem
{
    public const int MaxTitleLength = 25;
    public const int MaxContentLength = 2048;
    public const int MaxCommentLength = 512;
}

[Serializable, NetSerializable]
public struct NewsComment
{
    [ViewVariables(VVAccess.ReadWrite)]
    public string Content;

    [ViewVariables(VVAccess.ReadWrite)]
    public string? Author;

    [ViewVariables]
    public TimeSpan CommentTime;

    public NewsComment(string content, string? author, TimeSpan commentTime)
    {
        Content = content;
        Author = author;
        CommentTime = commentTime;
    }
}

[Serializable, NetSerializable]
public struct NewsArticle
{
    [ViewVariables(VVAccess.ReadWrite)]
    public string Title;

    [ViewVariables(VVAccess.ReadWrite)]
    public string Content;

    [ViewVariables(VVAccess.ReadWrite)]
    public string? Author;

    [ViewVariables]
    public ICollection<(NetEntity, uint)>? AuthorStationRecordKeyIds;

    [ViewVariables]
    public TimeSpan ShareTime;

    [ViewVariables(VVAccess.ReadWrite)]
    public int Likes;

    [ViewVariables(VVAccess.ReadWrite)]
    public int Dislikes;

    [ViewVariables(VVAccess.ReadWrite)]
    public List<NewsComment> Comments;

    [ViewVariables(VVAccess.ReadWrite)]
    public List<string> LikedBy;

    [ViewVariables(VVAccess.ReadWrite)]
    public List<string> DislikedBy;

    public NewsArticle(string title, string content, string? author, TimeSpan shareTime)
    {
        Title = title;
        Content = content;
        Author = author;
        ShareTime = shareTime;
        Likes = 0;
        Dislikes = 0;
        Comments = new List<NewsComment>();
        LikedBy = new List<string>();
        DislikedBy = new List<string>();
    }
}

[ByRefEvent]
public record struct NewsArticlePublishedEvent(NewsArticle Article);

[ByRefEvent]
public record struct NewsArticleDeletedEvent;
