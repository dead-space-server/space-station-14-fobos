using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.TEMP_FOR_EVENT.Alien_Isolation.Weeds;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WeedComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Range = 4;

    [DataField, AutoNetworkedField]
    public EntProtoId Spawns = "EventXenoWeed";

    [DataField("attachedWall")]
    public EntityUid? AttachedWall;

    [DataField, AutoNetworkedField]
    public bool IsSource = true;

    [DataField, AutoNetworkedField]
    public EntityUid? Source;

    [DataField, AutoNetworkedField]
    public int Level = 1;

    [DataField, AutoNetworkedField]
    public bool BlockOtherWeeds = false;

    [DataField]
    public List<EntityUid> Spread = new();
}
