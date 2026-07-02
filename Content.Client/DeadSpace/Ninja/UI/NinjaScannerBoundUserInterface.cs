using Robust.Client.UserInterface;
using Content.Shared.DeadSpace.Ninja.Components;

namespace Content.Client.DeadSpace.Ninja.UI;

public sealed class NinjaScannerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private NinjaScannerWindow? _menu;

    public NinjaScannerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<NinjaScannerWindow>();
        _menu.OnApplyDisguise += target => SendMessage(new NinjaApplyDisguiseMessage(target));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not NinjaScannerBoundUserInterfaceState s)
            return;

        _menu?.UpdateState(s.Targets);
    }
}