using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class ScannerProgramUiState : BoundUserInterfaceState
{
    public List<ScannedDocument> Documents;
    public int SelectedIndex;

    public ScannerProgramUiState(List<ScannedDocument> documents, int selectedIndex)
    {
        Documents = documents;
        SelectedIndex = selectedIndex;
    }
}
