using Content.Shared.Actions;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.IPC;

[RegisterComponent, NetworkedComponent]
public sealed partial class IPCComponent : Component
{
    public const short MaxBatteryAlertLevels = 10;

    [DataField]
    [ViewVariables]
    public float BatteryLowThreshold = 0.01f;

    [DataField]
    [ViewVariables]
    public float MovementPenalty = 0.2f;

    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";

    [DataField]
    public EntProtoId DrainBatteryAction = "ActionDrainBattery";

    [DataField]
    [ViewVariables]
    public EntityUid? ActionEntity;

    [ViewVariables]
    public bool DrainActivated;

    [DataField]
    [ViewVariables]
    public float IdleDrainRate = 2.5f;

    [ViewVariables]
    public short LastBatteryLevel;
}

public sealed partial class ToggleDrainActionEvent : InstantActionEvent
{
}
