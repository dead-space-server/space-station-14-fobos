using Content.Client.UserInterface.Fragments;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.CartridgeLoader.Cartridges;

public sealed partial class SnakeCartridgeUi : UIFragment
{
    private SnakeCartridgeFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new SnakeCartridgeFragment();
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
    }
}
