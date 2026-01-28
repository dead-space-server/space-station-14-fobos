using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HeatSourceComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Radius = 3;

    [DataField, AutoNetworkedField]
    public float Power = 500f;

    [DataField, AutoNetworkedField]
    public bool Enabled;

    [DataField, AutoNetworkedField]
    public float TargetTemperature = 293f;

    [DataField]
    public float UpdateInterval = 1f;

    public float NextUpdate;
}
