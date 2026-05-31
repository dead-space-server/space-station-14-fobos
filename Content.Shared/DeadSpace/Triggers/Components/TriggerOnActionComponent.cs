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
    /// <summary>
    /// If true, action added to both this entity and its parent entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Parent = false;

    /// <summary>
    /// If true, delete component after use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool DeleteComponentAfterTrigger = false;

    /// <summary>
    /// The action to add.
    /// This must raise <see cref="TriggerActionEvent"/> to then get handled.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId<InstantActionComponent> Action;

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
}
public sealed partial class TriggerActionEvent : InstantActionEvent;