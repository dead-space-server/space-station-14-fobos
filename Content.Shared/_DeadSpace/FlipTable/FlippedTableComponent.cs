using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DeadSpace.FlipTable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FlippedTableComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public EntProtoId OriginalTableId = default!;

    [DataField, AutoNetworkedField]
    public EntityUid? FlipperUid;
}
