using Robust.Shared.GameStates;

namespace Content.Shared.Vehicle.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleKeyComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? BoundVehicle;
}