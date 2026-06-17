using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Camera;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ScreenshakeComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<ScreenshakeCommand> Commands = [];

    public override bool SendOnlyToOwner => true;
}

[DataRecord, Serializable, NetSerializable]
public sealed partial record ScreenshakeCommand(
    ScreenshakeParameters? Translation,
    ScreenshakeParameters? Rotation,
    TimeSpan Start,
    TimeSpan End);

[DataDefinition, Serializable, NetSerializable]
public sealed partial record ScreenshakeParameters
{
    [DataField(required: true)]
    public float Trauma;

    [DataField]
    public float DecayRate = 1.2f;

    [DataField]
    public float Frequency = 0.01f;
}
