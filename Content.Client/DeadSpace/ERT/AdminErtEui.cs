// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Administration.UI.Tabs.AdminTab;
using Content.Client.Eui;
using Content.Shared.DeadSpace.ERT;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client.DeadSpace.ERT;

[UsedImplicitly]
public sealed class AdminErtEui : BaseEui
{
    private readonly ERTCallWindow _window;

    public AdminErtEui()
    {
        _window = new ERTCallWindow();
        _window.OnClose += () => SendMessage(new AdminErtEuiMsg.Close());
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
        // no state currently
    }
}
