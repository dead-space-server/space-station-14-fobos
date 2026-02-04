using Content.Shared._Frostheim.Radar;
using Robust.Client.UserInterface;

namespace Content.Client._Frostheim.Radar;

public sealed class FrostheimRadarBoundUserInterface : BoundUserInterface
{
    private FrostheimRadarWindow? _window;

    public FrostheimRadarBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        _window = this.CreateWindow<FrostheimRadarWindow>();
        _window.OpenCentered();
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (message is FrostheimRadarUpdateMessage update)
            _window?.UpdateState(update);
    }
}
