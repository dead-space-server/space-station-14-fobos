// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
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
        base.Open();
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
