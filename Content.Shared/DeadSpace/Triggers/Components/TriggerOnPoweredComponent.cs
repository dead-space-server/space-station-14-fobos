using Robust.Shared.GameStates;
using Content.Shared.Trigger.Components.Triggers;

namespace Content.Shared.DeadSpace.Triggers.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnPoweredComponent : BaseTriggerOnXComponent
{
    [DataField, AutoNetworkedField]
    public bool TriggerIfGetPower = true;

    [DataField, AutoNetworkedField]
    public bool TriggerIfLosePower = true;
}
