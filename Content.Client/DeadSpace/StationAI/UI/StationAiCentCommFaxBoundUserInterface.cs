// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.StationAI.UI;
using JetBrains.Annotations;

namespace Content.Client.DeadSpace.StationAI.UI;

[UsedImplicitly]
public sealed class StationAiCentCommFaxBoundUserInterface : BoundUserInterface
{
    private StationAiCentCommFaxWindow? _window;

    public StationAiCentCommFaxBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window?.Close();
        _window = new StationAiCentCommFaxWindow();
        _window.OpenCentered();
        _window.OnClose += Close;
        _window.SubmitPressed += OnSubmitPressed;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is StationAiCentCommFaxUiState faxState)
            _window?.SetState(faxState);
    }

    private void OnSubmitPressed(string content)
    {
        SendMessage(new StationAiCentCommFaxSendMessage(content));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        if (_window != null)
            _window.SubmitPressed -= OnSubmitPressed;

        _window?.Parent?.RemoveChild(_window);
    }
}
