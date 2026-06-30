using Robust.Shared.GameStates;
using Content.Shared.Trigger.Components.Triggers;
using Content.Shared.DeadSpace.CodeLock;

namespace Content.Shared.DeadSpace.Triggers.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnCodeLockStatusComponent : BaseTriggerOnXComponent
{
    [DataField, AutoNetworkedField]
    public CodeLockStatus Status = CodeLockStatus.COOLDOWN;

}
