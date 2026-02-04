using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Frostheim.Map;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FrostMapComponent : Component
{
    [DataField, AutoNetworkedField]
    public FrostheimWeather CurrentWeather = FrostheimWeather.None;

    [DataField]
    public float MinWeatherInterval = 90f;

    [DataField]
    public float MaxWeatherInterval = 240f;

    [DataField]
    public TimeSpan NextWeatherChange = TimeSpan.Zero;

    [DataField]
    public int SpawnThrusters = 6;

    [DataField]
    public int SpawnGyroscopes = 2;

    [DataField]
    public EntProtoId BrokenThrusterProto = "FrostheimBrokenThruster";

    [DataField]
    public EntProtoId BrokenGyroscopeProto = "FrostheimBrokenGyroscope";

    [ViewVariables]
    public bool LootSpawned;

    [ViewVariables]
    public bool CreaturesSpawned;

    [ViewVariables]
    public bool BruteSpawned;

    [ViewVariables]
    public TimeSpan CreatureSpawnTime;

    [ViewVariables]
    public TimeSpan NextRespawnCheck;
}
