// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.ERT.Prototypes;
using Content.Shared.Shuttles.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.ERT;

[Serializable, NetSerializable]
public sealed class ErtResponseConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public List<ProtoId<ErtTeamPrototype>> Teams = new();
    public int Money = new();
    public bool IsAuthorized = true;
    public string? AuthorizationMessage;

    public ErtResponseConsoleBoundUserInterfaceState(
        List<ProtoId<ErtTeamPrototype>> teams,
        int money,
        bool isAuthorized = true,
        string? authorizationMessage = null)
    {
        Teams = teams;
        Money = money;
        IsAuthorized = isAuthorized;
        AuthorizationMessage = authorizationMessage;
    }
}


[Serializable, NetSerializable]
public sealed class ErtResponseConsoleUiButtonPressedMessage : BoundUserInterfaceMessage
{
    public readonly ErtResponseConsoleUiButton Button;
    public string? Team;
    public string? CallReason;

    public ErtResponseConsoleUiButtonPressedMessage(
        ErtResponseConsoleUiButton button,
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
public enum ErtResponseConsoleUiButton : byte
{
    ResponseErt
}

[Serializable, NetSerializable]
public enum ErtResponseConsoleUiKey : byte
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
