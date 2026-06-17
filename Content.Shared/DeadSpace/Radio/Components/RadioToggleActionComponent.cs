using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;

namespace Content.Shared.DeadSpace.Radio.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RadioToggleActionComponent : Component
{
    /// <summary>
    /// The toggle action.
    /// This must raise <see cref="RadioToggleEvent"/> to then get handled.
    /// </summary>
    [DataField]
    public EntProtoId<InstantActionComponent> Action = "RadioToggleAction";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
    [DataField]
    public bool Enabled = false;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RadioToggleActionMarkerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Radio;
}

public sealed partial class RadioToggleEvent : InstantActionEvent;