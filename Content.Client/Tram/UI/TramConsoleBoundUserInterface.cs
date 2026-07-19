using Content.Shared.Tram.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Tram.UI;

[UsedImplicitly]
public sealed class TramConsoleBoundUserInterface : BoundUserInterface
{
    private TramConsoleWindow? _window;

    public TramConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<TramConsoleWindow>();
        _window.DestinationSelected += OnDestinationSelected;
    }

    private void OnDestinationSelected(string destinationId)
    {
        SendMessage(new TramConsoleSelectDestinationMessage
        {
            DestinationId = destinationId,
        });
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not TramConsoleBoundUserInterfaceState cState)
            return;

        _window?.UpdateState(cState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }
}
