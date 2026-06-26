using Content.Shared.Paper;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class ScannedDocument
{
    public string Name;
    public string Content;
    public List<StampDisplayInfo> StampedBy;
    public List<string> Signatures = new();

    public ScannedDocument(string name, string content, List<StampDisplayInfo> stampedBy, List<string>? signatures = null)
    {
        Name = name;
        Content = content;
        StampedBy = stampedBy;
        Signatures = signatures ?? new();
    }
}
