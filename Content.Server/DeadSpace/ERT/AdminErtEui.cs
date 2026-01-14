// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.EUI;
using Content.Shared.DeadSpace.ERT;
using Content.Shared.Eui;

namespace Content.Server.DeadSpace.ERT;

public sealed class AdminErtEui : BaseEui
{
    public override void Opened()
    {
        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        return new AdminErtEuiState();
    }

    // no messages to handle for now
}
