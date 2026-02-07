// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.ERT.Prototypes;
using Content.Shared.Shuttles.Systems;
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
    public string? Team;
    public string? CallReason;

    public ErtResponceConsoleUiButtonPressedMessage(
        ErtResponceConsoleUiButton button,
        string? team = null,
        string? callReason = null
        )
    {
        Button = button;
        Team = team;
        CallReason = callReason;
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

// ErtComputerShuttle

[Serializable, NetSerializable]
public sealed class ErtComputerShuttleBoundUserInterfaceState : BoundUserInterfaceState
{ }

[Serializable, NetSerializable]
public sealed class ErtComputerShuttleUiButtonPressedMessage : BoundUserInterfaceMessage
{
    public readonly ErtComputerShuttleUiButton Button;

    public ErtComputerShuttleUiButtonPressedMessage(
        ErtComputerShuttleUiButton button
        )
    {
        Button = button;
    }
}

[Serializable, NetSerializable]
public enum ErtComputerShuttleUiButton : byte
{
    Evacuation,
    CancelEvacuation
}

[Serializable, NetSerializable]
public enum ErtComputerShuttleUiKey : byte
{
    Key
}
