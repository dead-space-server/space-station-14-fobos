using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared.DeadSpace.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.CartridgeLoader.Cartridges;

public sealed partial class ScannerProgramUi : UIFragment
{
    private ScannerProgramUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new ScannerProgramUiFragment();
        _fragment.OnDocumentDeleted += index =>
        {
            var message = new ScannerProgramUiMessageEvent(ScannerProgramUiAction.Delete, index);
            userInterface.SendMessage(new CartridgeUiMessage(message));
        };
        _fragment.OnAllDeleted += () =>
        {
            var message = new ScannerProgramUiMessageEvent(ScannerProgramUiAction.Clear);
            userInterface.SendMessage(new CartridgeUiMessage(message));
        };
        _fragment.OnDocumentRenamed += (index, newName) =>
        {
            var message = new ScannerProgramUiMessageEvent(ScannerProgramUiAction.Rename, index, newName);
            userInterface.SendMessage(new CartridgeUiMessage(message));
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not ScannerProgramUiState scannerState)
            return;

        _fragment?.UpdateState(scannerState.Documents, scannerState.SelectedIndex);
    }
}
