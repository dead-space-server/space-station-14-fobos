using Content.Shared.MassMedia.Systems;
using Robust.Shared.Network;

namespace Content.Shared.MassMedia.Components;

[RegisterComponent]
public sealed partial class StationNewsComponent : Component
{
    [DataField]
    public List<NewsArticle> Articles = new();

    // DS14
    [ViewVariables]
    public Dictionary<NetUserId, TimeSpan> LastCommentTimes = new();
}
