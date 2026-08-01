using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Clothing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MultiClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Force;

    [DataField, AutoNetworkedField]
    public Dictionary<string, EntProtoId> Equipment = new();

    [DataField, AutoNetworkedField]
    public Dictionary<string, EntityUid> SpawnedItems = new();

    [DataField, AutoNetworkedField]
    public Dictionary<string, EntityUid> ForcedOffItems = new();
}