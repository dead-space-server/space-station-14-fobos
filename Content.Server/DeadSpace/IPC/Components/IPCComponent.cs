// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.IPC.Components;

[RegisterComponent]
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
    [ViewVariables]
    public float IdleDrainRate = 2.5f;

    [DataField(readOnly: true)]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    [DataField(readOnly: true)]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";

    [DataField(readOnly: true)]
    public EntProtoId DrainBatteryAction = "ActionDrainBattery";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionEntity;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool DrainActivated;

    [ViewVariables(VVAccess.ReadOnly)]
    public short LastBatteryLevel;
}
