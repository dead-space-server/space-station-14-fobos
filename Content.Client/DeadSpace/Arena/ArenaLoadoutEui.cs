using Content.Client.Eui;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client.DeadSpace.Arena;

[UsedImplicitly]
public sealed class ArenaLoadoutEui : BaseEui
{
    private readonly ArenaLoadoutWindow _window;

    public ArenaLoadoutEui()
    {
        _window = new ArenaLoadoutWindow();
        _window.OnClose += () => SendMessage(new CloseEuiMessage());
        _window.OnLoadoutConfirmed += weaponIdx =>
        {
            SendMessage(new ArenaLoadoutSelectedMessage(weaponIdx));
        };
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not ArenaLoadoutEuiState loadoutState)
            return;

        _window.UpdateState(loadoutState);
    }
}
