using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FrostMapComponent : Component
{
    [DataField, AutoNetworkedField]
    public FrostheimWeather CurrentWeather = FrostheimWeather.None;
}
