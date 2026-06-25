using Content.Shared.DeadSpace.CartridgeLoader.Cartridges;

namespace Content.Server.DeadSpace.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class ScannerProgramComponent : Component
{
    [DataField]
    public int MaxDocuments = 10;

    [DataField]
    public int MaxDocumentNameLength = 64;

    [DataField]
    public List<ScannedDocument> Documents = new();
}
