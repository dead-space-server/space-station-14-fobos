// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.ERT.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.ERT;

[Serializable, NetSerializable]
public sealed class ErtResponceConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public List<ProtoId<ErtTeamPrototype>> Teams = new();
    public int Money = new();

    public ErtResponceConsoleBoundUserInterfaceState(List<ProtoId<ErtTeamPrototype>> teams, int money)
    {
        Teams = teams;
        Money = money;
    }
}


[Serializable, NetSerializable]
public sealed class ErtResponceConsoleUiButtonPressedMessage : BoundUserInterfaceMessage
{
    public readonly ErtResponceConsoleUiButton Button;
    public ProtoId<ErtTeamPrototype>? Team;

    public ErtResponceConsoleUiButtonPressedMessage(
        ErtResponceConsoleUiButton button,
        ProtoId<ErtTeamPrototype>? team = null
        )
    {
        Button = button;
        Team = team;
    }
}


[Serializable, NetSerializable]
public enum ErtResponceConsoleUiButton : byte
{
    ResponceErt
}

[Serializable, NetSerializable]
public enum ErtResponceConsoleUiKey : byte
{
    Key
}