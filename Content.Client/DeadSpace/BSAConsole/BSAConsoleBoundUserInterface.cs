using Content.Shared.DeadSpace.BSAConsole;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Map;

namespace Content.Client.DeadSpace.BSAConsole;

[UsedImplicitly]
public sealed class BSAConsoleBoundUserInterface : BoundUserInterface
{
    private BSAConsoleWindow? _window;

    public BSAConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<BSAConsoleWindow>();
        _window.OnFirePressed += OnFire;
        _window.OnSwitchViewPressed += OnSwitchView;
        _window.OnSelectGridPressed += OnSelectGrid;
        _window.OnEjectDiskPressed += OnEjectDisk;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not BSAConsoleUiState bsaState)
            return;

        _window?.UpdateState(bsaState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }

    private void OnFire(MapCoordinates mapCoords)
    {
        SendMessage(new BSAConsoleFireMessage((float)mapCoords.X, (float)mapCoords.Y, (int)mapCoords.MapId));
    }

    private void OnSwitchView(string viewMode)
    {
        SendMessage(new BSAConsoleSwitchViewMessage(viewMode));
    }

    private void OnSelectGrid(string gridName)
    {
        SendMessage(new BSAConsoleSelectGridMessage(gridName));
    }

    private void OnEjectDisk()
    {
        SendMessage(new BSAConsoleEjectDiskMessage());
    }
}
