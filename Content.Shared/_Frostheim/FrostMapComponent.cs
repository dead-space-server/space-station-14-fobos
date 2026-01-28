using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FrostMapComponent : Component
{
    [DataField, AutoNetworkedField]
    public FrostheimWeather CurrentWeather = FrostheimWeather.None;

    [DataField]
    public float MinWeatherInterval = 300f;

    [DataField]
    public float MaxWeatherInterval = 900f;

    [DataField]
    public TimeSpan NextWeatherChange = TimeSpan.Zero;
}
