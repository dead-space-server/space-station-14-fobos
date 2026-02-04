using Content.Shared._Frostheim.Shuttle;
using Robust.Client.UserInterface;

namespace Content.Client._Frostheim.Shuttle;

public sealed class FrostheimNavConsoleBoundUserInterface : BoundUserInterface
{
    private FrostheimNavConsoleWindow? _window;

    public FrostheimNavConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<FrostheimNavConsoleWindow>();
        _window.OnPathClick += (x, y) => SendMessage(new NavConsolePathClickMessage(x, y));
        _window.OnResetPath += () => SendMessage(new NavConsoleResetPathMessage());
        _window.OnCalibrate += (idx, val) => SendMessage(new NavConsoleCalibrateMessage(idx, val));
        _window.OnConfirm += () => SendMessage(new NavConsoleConfirmMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is FrostheimNavConsoleState navState)
            _window?.UpdateState(navState);
    }
}
