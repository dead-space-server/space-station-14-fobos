using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Tram.Components;

/// <summary>
/// Component for the tram control console.
/// Shows tram location and allows calling/operating it.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedTramSystem))]
public sealed partial class TramConsoleComponent : Component
{
    /// <summary>
    /// The station ID this console is associated with.
    /// Used to determine which station the console is at.
    /// </summary>
    [DataField]
    public string? ConsoleStationId;

    /// <summary>
    /// Whether this is a call-only console (platform) or a full control console (inside tram).
    /// </summary>
    [DataField]
    public bool IsCallOnly = true;

    /// <summary>
    /// Reference to the tram car entity this console controls.
    /// Found automatically at runtime if null.
    /// </summary>
    [DataField]
    public EntityUid? TramCar;
}

[Serializable, NetSerializable]
public enum TramConsoleUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class TramConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// Is the tram currently moving?
    /// </summary>
    public bool IsMoving;

    /// <summary>
    /// Is the tram unreachable (broken/no power)?
    /// </summary>
    public bool IsBroken;

    /// <summary>
    /// The current location of the tram (station ID), or null if in transit.
    /// </summary>
    public string? CurrentLocation;

    /// <summary>
    /// Available destinations with their IDs and display names.
    /// </summary>
    public List<TramDestinationInfo> Destinations = new();

    /// <summary>
    /// Whether the controls are locked.
    /// </summary>
    public bool ControlsLocked;
}

[Serializable, NetSerializable]
public sealed class TramDestinationInfo
{
    public string Id = string.Empty;
    public string Name = string.Empty;
    public Dictionary<string, string> Icons = new();
    public bool IsHere;
}

[Serializable, NetSerializable]
public sealed class TramConsoleSelectDestinationMessage : BoundUserInterfaceMessage
{
    public string DestinationId = string.Empty;
}
