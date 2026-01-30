using Content.Shared.DeadSpace.RiotSign;
using JetBrains.Annotations;

namespace Content.Client.DeadSpace.RiotSign;

[UsedImplicitly]
public sealed class LabelBoundUserInterface : BoundUserInterface
{
    private LabelWindow? _window;

    public LabelBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = new LabelWindow();

        _window.OnClose += () =>
        {
            if (_window != null)
            {
                SendMessage(new LabelChangedMessage(_window.TypedText));
            }

            Close();
        };

        _window.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _window?.Close();
        }
    }
}
