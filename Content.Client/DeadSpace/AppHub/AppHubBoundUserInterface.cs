using Content.Shared.DeadSpace.AppHub;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.AppHub;

[UsedImplicitly]
public sealed class AppHubBoundUserInterface : BoundUserInterface
{
    private AppHubMenu? _window;

    public AppHubBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<AppHubMenu>();
        _window.OnInstallPressed += OnInstall;
        _window.OnUninstallPressed += OnUninstall;
        _window.OnCategorySelected += OnCategorySelected;
        _window.OnEjectPdaPressed += OnEjectPda;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not AppHubUiState appHubState)
            return;

        _window?.UpdateState(appHubState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }

    private void OnInstall(string programId)
    {
        SendMessage(new AppHubInstallMessage(programId));
    }

    private void OnUninstall(string programId)
    {
        SendMessage(new AppHubUninstallMessage(programId));
    }

    private void OnCategorySelected(string category)
    {
        SendMessage(new AppHubSelectCategoryMessage(category));
    }

    private void OnEjectPda()
    {
        SendMessage(new AppHubEjectPdaMessage());
    }
}
