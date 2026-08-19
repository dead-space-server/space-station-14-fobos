using Robust.Shared.GameStates;

namespace Content.Shared.PipeShuttle.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PipeShuttleStopComponent : Component
{
    [DataField("stopId"), AutoNetworkedField]
    public string StopId = string.Empty;

    [DataField("stopName"), AutoNetworkedField]
    public string StopName = string.Empty;

    [DataField("linkedDoors")]
    public List<EntityUid> LinkedDoors = new();
}
