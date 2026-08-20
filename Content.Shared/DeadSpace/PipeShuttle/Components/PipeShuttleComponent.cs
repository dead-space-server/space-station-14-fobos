using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using System.Numerics;

namespace Content.Shared.DeadSpace.PipeShuttle.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PipeShuttleComponent : Component
{
    [DataField("destinations")]
    public List<PipeShuttleDestination> Destinations = new();

    [DataField("currentDestId"), AutoNetworkedField]
    public string? CurrentDestId;

    [DataField("targetDestId"), AutoNetworkedField]
    public string? TargetDestId;

    [DataField("travelling"), AutoNetworkedField]
    public bool Travelling;

    [DataField("moveSpeed")]
    public float MoveSpeed = 8f;

    [DataField("arrivalThreshold")]
    public float ArrivalThreshold = 0.5f;

    [DataField("cooldown")]
    public float Cooldown = 10f;

    [DataField("positionOffset")]
    public Vector2 PositionOffset;
}

[Serializable, DataDefinition, NetSerializable]
public sealed partial class PipeShuttleDestination
{
    [DataField("id")]
    public string Id = string.Empty;

    [DataField("name")]
    public string Name = string.Empty;

    [DataField("position")]
    public Vector2 Position;
}
