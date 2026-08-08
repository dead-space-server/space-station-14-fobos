using Content.Shared.Medical.SuitSensor;

namespace Content.Server.Medical.CrewMonitoring;

[AutoGenerateComponentPause]
[RegisterComponent]
[Access(typeof(CrewMonitoringConsoleSystem))]
public sealed partial class CrewMonitoringConsoleComponent : Component
{
    /// <summary>
    ///     List of all currently connected sensors to this console.
    /// </summary>
    public Dictionary<string, SuitSensorStatus> ConnectedSensors = new();

    /// <summary>
    ///     After what time sensor consider to be lost.
    /// </summary>
    [DataField("sensorTimeout"), ViewVariables(VVAccess.ReadWrite)]
    public float SensorTimeout = 10f;

    [DataField]
    [AutoPausedField]
    public TimeSpan NextSound = TimeSpan.Zero;

    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(10);
    public CrewMonitoringConsolePingMode CurrentPingMode = new();

    public List<CrewMonitoringConsolePingMode> PingModes = new()
    {
        CrewMonitoringConsolePingMode.Health4,
        CrewMonitoringConsolePingMode.Krit,
        CrewMonitoringConsolePingMode.Dead,
    };
}

public enum CrewMonitoringConsolePingMode
{
    Health4,
    Krit,
    Dead,
    Disabled
}