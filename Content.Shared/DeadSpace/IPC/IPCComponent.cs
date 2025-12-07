using Content.Shared.Actions;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.IPC;

[RegisterComponent, NetworkedComponent]
public sealed partial class IPCComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";

    [DataField]
    public EntProtoId DrainBatteryAction = "ActionDrainBattery";

    [DataField]
    public EntityUid? ActionEntity;

    public bool DrainActivated;

    [DataField]
    public float IdleDrainRate = 2.5f;

    [DataField]
    public int EmpDamage = 30;
}

public sealed partial class ToggleDrainActionEvent : InstantActionEvent
{
}
