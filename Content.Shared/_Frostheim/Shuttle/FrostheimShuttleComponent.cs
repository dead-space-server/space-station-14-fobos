using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim.Shuttle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FrostheimShuttleComponent : Component
{
    [DataField, AutoNetworkedField]
    public int AdditionalThrustersNeeded = 4;

    [DataField, AutoNetworkedField]
    public bool RequireGyroscope = true;

    [ViewVariables, AutoNetworkedField]
    public int InitialThrusters;

    [ViewVariables, AutoNetworkedField]
    public int CurrentThrusters;

    [ViewVariables, AutoNetworkedField]
    public bool HasGyroscope;

    [ViewVariables, AutoNetworkedField]
    public bool Ready;

    [ViewVariables]
    public bool Initialized;
}
