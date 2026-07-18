// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.FlipTable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FlippableTableComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public EntProtoId FlippedTableId = default!;

    [DataField]
    public float FlipDelay = 2.0f;
}
