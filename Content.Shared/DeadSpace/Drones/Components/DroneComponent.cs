using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Drones.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DroneComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? DroneController;

    [DataField, AutoNetworkedField]
    public EntityUid? DroneHost;

    [DataField]
    public bool CanConnect = true;

    [DataField]
    public bool IsFPV = true;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("wattage")]
    public float Wattage { get; set; } = .8f;
    [DataField]
    public List<DroneChargeWarning> ChargeWarnings = new()
    {
        new DroneChargeWarning { Threshold = 0.50f, String = "drone-charge-warning-50" },
        new DroneChargeWarning { Threshold = 0.15f, String = "drone-charge-warning-15" },
    };

    [DataField, AutoNetworkedField, AlwaysPushInheritance]
    public List<EntProtoId> ActionsToDrone = new();

    [DataField, AutoNetworkedField]
    public List<EntityUid> ActionEntities = new();
}

[DataDefinition]
public sealed partial class DroneChargeWarning
{
    [DataField]
    public float Threshold;

    [DataField]
    public string String;

    public bool Triggered;
}