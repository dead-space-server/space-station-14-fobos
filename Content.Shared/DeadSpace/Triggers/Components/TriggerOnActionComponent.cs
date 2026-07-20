using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Trigger.Components.Triggers;

namespace Content.Shared.DeadSpace.Triggers.Components;

/// <summary>
/// Triggers an entity when it is use action.
/// The user is the entity that was used action.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnActionComponent : BaseTriggerOnXComponent
{
    [DataField]
    public bool RemoveAfterActive = false;
}
public sealed partial class TriggerActionEvent : InstantActionEvent;