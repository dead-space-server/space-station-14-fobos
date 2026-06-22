using Content.Shared.Paper;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class ScannedDocument
{
    public string Name;
    public string Content;
    public List<StampDisplayInfo> StampedBy;

    public ScannedDocument(string name, string content, List<StampDisplayInfo> stampedBy)
    {
        Name = name;
        Content = content;
        StampedBy = stampedBy;
    }
}
