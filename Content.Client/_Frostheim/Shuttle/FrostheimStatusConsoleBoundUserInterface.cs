using Content.Shared._Frostheim.Shuttle;
using Robust.Client.UserInterface;

namespace Content.Client._Frostheim.Shuttle;

public sealed class FrostheimStatusConsoleBoundUserInterface : BoundUserInterface
{
    private FrostheimStatusConsoleWindow? _window;

    public FrostheimStatusConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<FrostheimStatusConsoleWindow>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is FrostheimStatusConsoleState statusState)
            _window?.UpdateState(statusState);
    }
}
