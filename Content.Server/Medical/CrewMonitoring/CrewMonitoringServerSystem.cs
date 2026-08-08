using Content.Server.DeviceNetwork.Systems;
using Content.Server.Medical.SuitSensors;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.Medical.SuitSensor;
using Robust.Shared.Timing;
using Content.Shared.DeviceNetwork.Components;

namespace Content.Server.Medical.CrewMonitoring;

public sealed class CrewMonitoringServerSystem : EntitySystem
{
    [Dependency] private readonly SuitSensorSystem _sensors = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly SingletonDeviceNetServerSystem _singletonServerSystem = default!;

    private const float UpdateRate = 3f;
    private float _updateDiff;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrewMonitoringServerComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<CrewMonitoringServerComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<CrewMonitoringServerComponent, DeviceNetServerDisconnectedEvent>(OnDisconnected);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // check update rate
        _updateDiff += frameTime;
        if (_updateDiff < UpdateRate)
            return;
        _updateDiff -= UpdateRate;

        var servers = EntityQueryEnumerator<CrewMonitoringServerComponent>();

        while (servers.MoveNext(out var id, out var server))
        {
            if (!_singletonServerSystem.IsActiveServer(id))
                continue;

            UpdateTimeout(id);
            BroadcastSensorStatus(id, server);
        }
    }

    /// <summary>
    /// Adds or updates a sensor status entry if the received package is a sensor status update
    /// </summary>
    private void OnPacketReceived(EntityUid uid, CrewMonitoringServerComponent component, DeviceNetworkPacketEvent args)
    {
        var sensorStatus = _sensors.PacketToSuitSensor(args.Data);
        if (sensorStatus == null)
            return;

        if (sensorStatus.Coordinates != null && NeedPing(sensorStatus, component.SensorStatus.GetValueOrDefault(args.SenderAddress), out var pingmode))
        {
            Send(uid, pingmode);
        }
        sensorStatus.Timestamp = _gameTiming.CurTime;
        component.SensorStatus[args.SenderAddress] = sensorStatus;
    }

    public bool NeedPing(SuitSensorStatus newsensor, SuitSensorStatus? oldsensor, out CrewMonitoringConsolePingMode? pingmode)
    {
        pingmode = null;
        if (newsensor.IsAlive)
        {
            if (newsensor.DamagePercentage != null && newsensor.DamagePercentage.Value > 0.6)
            {
                if (newsensor.DamagePercentage != null && newsensor.DamagePercentage.Value > 1)
                {
                    if (oldsensor == null || (oldsensor.DamagePercentage != null && oldsensor.DamagePercentage.Value < 1))
                    {
                        pingmode = CrewMonitoringConsolePingMode.Krit;
                        return true;
                    }
                }
                else if (oldsensor == null || (oldsensor.DamagePercentage != null && oldsensor.DamagePercentage.Value < 0.6))
                {
                    pingmode = CrewMonitoringConsolePingMode.Health4;
                    return true;
                }
            }
            else
            {
                return false;
            }

        }
        else if (oldsensor == null || oldsensor.IsAlive)
        {
            pingmode = CrewMonitoringConsolePingMode.Dead;
            return true;
        }
        return false;
    }
    public void Send(EntityUid uid, CrewMonitoringConsolePingMode? pingmode)
    {
        var payload = new NetworkPayload()
        {
            ["PingMode"] = pingmode
        };
        _deviceNetworkSystem.QueuePacket(uid, null, payload);
    }

    /// <summary>
    /// Clears the servers sensor status list
    /// </summary>
    private void OnRemove(EntityUid uid, CrewMonitoringServerComponent component, ComponentRemove args)
    {
        component.SensorStatus.Clear();
    }

    /// <summary>
    /// Drop the sensor status if it hasn't been updated for to long
    /// </summary>
    private void UpdateTimeout(EntityUid uid, CrewMonitoringServerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        foreach (var (address, sensor) in component.SensorStatus)
        {
            var dif = _gameTiming.CurTime - sensor.Timestamp;
            if (dif.Seconds > component.SensorTimeout)
                component.SensorStatus.Remove(address);
        }
    }

    /// <summary>
    /// Broadcasts the status of all connected sensors
    /// </summary>
    private void BroadcastSensorStatus(EntityUid uid, CrewMonitoringServerComponent? serverComponent = null, DeviceNetworkComponent? device = null)
    {
        if (!Resolve(uid, ref serverComponent, ref device))
            return;

        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = DeviceNetworkConstants.CmdUpdatedState,
            [SuitSensorConstants.NET_STATUS_COLLECTION] = serverComponent.SensorStatus
        };

        _deviceNetworkSystem.QueuePacket(uid, null, payload, device: device);
    }

    /// <summary>
    /// Clears sensor data on disconnect
    /// </summary>
    private void OnDisconnected(EntityUid uid, CrewMonitoringServerComponent component, ref DeviceNetServerDisconnectedEvent _)
    {
        component.SensorStatus.Clear();
    }
}
