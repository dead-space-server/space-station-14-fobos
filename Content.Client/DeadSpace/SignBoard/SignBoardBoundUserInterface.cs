using Content.Client.DeadSpace.SignBoard.UI;
using Content.Shared.DeadSpace.SignBoard;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.SignBoard;

public sealed class SignBoardBoundUserInterface : BoundUserInterface
{
    private SignBoardWindow? _window;

    public SignBoardBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        _window = this.CreateWindow<SignBoardWindow>();
        _window.OpenCentered();
        _window.OnTextSubmitted += OnTextSubmitted;
    }

    private void OnTextSubmitted(string text)
    {
        SendMessage(new SignBoardSetTextMessage(text));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is SignBoardBoundUserInterfaceState cast)
            _window?.UpdateState(cast.Text);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }
}
