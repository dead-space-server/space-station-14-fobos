using Content.Shared.DeadSpace.CartridgeLoader.Cartridges;

namespace Content.Server.DeadSpace.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class ScannerProgramComponent : Component
{
    [DataField]
    public List<ScannedDocument> Documents = new();
}
