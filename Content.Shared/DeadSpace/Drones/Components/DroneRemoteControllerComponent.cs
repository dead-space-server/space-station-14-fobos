using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;

namespace Content.Shared.DeadSpace.Drones.Components;

/// <summary>
/// 
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DroneRemoteControllerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedDrone;

    [DataField]
    public bool IsDroneConnected = false;
    [DataField]
    public bool ContolDrone = false;

    [DataField]
    public bool CanWorkOnDifferentMaps = false;

    [DataField]
    public int Range = 10;

    [DataField]
    public int WarningRange = 7;

    [DataField]
    public TimeSpan ConnectTime = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan WarningPeriod = TimeSpan.FromSeconds(3);

    [DataField]
    public TimeSpan NextWarningTime = TimeSpan.Zero;

    [DataField]
    public bool CanConnect = true;

    [DataField]
    public bool IsFPV = true;

    [DataField]
    public SoundSpecifier? ConnectSound = new SoundPathSpecifier("/Audio/_DeadSpace/Effects/GhostPingSounds/message2.ogg");

    [DataField]
    public SoundSpecifier? DisconnectSound = null;

    [DataField]
    public EntityWhitelist? ConnectWhitelist;
}

[Serializable, NetSerializable]
public sealed partial class TryDroneConnectDoAfterEvent : SimpleDoAfterEvent;