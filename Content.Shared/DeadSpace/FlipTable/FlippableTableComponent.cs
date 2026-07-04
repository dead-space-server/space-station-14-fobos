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

    [DataField]
    public float UnflipDelay = 1.0f;
}
