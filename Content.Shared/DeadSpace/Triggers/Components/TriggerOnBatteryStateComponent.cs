using Robust.Shared.GameStates;
using Content.Shared.Trigger.Components.Triggers;
using Content.Shared.Power.Components;

namespace Content.Shared.DeadSpace.Triggers.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnBatteryStateComponent : BaseTriggerOnXComponent
{
    [DataField, AutoNetworkedField]
    public BatteryState State = BatteryState.Neither;

    [DataField, AutoNetworkedField]
    public bool Triggered = false;
}
