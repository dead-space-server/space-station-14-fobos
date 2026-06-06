// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Administration;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class AdminGhostVisibilityComponent : Component
{
    [DataField]
    public EntProtoId ActionToggleVisibility = "ActionAGhostToggleVisibility";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionToggleVisibilityEntity;

    [DataField, AutoNetworkedField]
    public bool Hidden;

    public bool AddedHideContextMenuTag;
}

public sealed partial class AGhostToggleVisibilityActionEvent : InstantActionEvent;
