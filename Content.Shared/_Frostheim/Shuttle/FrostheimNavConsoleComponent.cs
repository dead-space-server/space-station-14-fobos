using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim.Shuttle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FrostheimNavConsoleComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool NavigationComplete;
}
