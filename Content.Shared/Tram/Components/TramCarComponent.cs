using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Tram.Components;

/// <summary>
/// Marks a grid as a tram car (the moving vehicle).
/// Contains data about the tram's current state and movement.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedTramSystem))]
public sealed partial class TramCarComponent : Component
{
    /// <summary>
    /// Is the tram currently travelling between stations?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Travelling;

    /// <summary>
    /// How many tiles the tram still needs to travel this tick.
    /// </summary>
    [DataField]
    public int TravelDistance;

    /// <summary>
    /// Direction the tram is currently moving.
    /// </summary>
    [DataField]
    public Direction TravelDirection = Direction.Invalid;

    /// <summary>
    /// The current station the tram is at (destination ID), or null if in transit.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? CurrentStation;

    /// <summary>
    /// The destination ID the tram is heading to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? TargetDestination;

    /// <summary>
    /// Time in seconds between each tile of movement.
    /// </summary>
    [DataField]
    public float MoveInterval = 0.5f;

    /// <summary>
    /// How many tiles to move per step (1 = tile-by-tile).
    /// </summary>
    [DataField]
    public int TilesPerStep = 1;

    /// <summary>
    /// Controls are locked while the tram is moving or during safety checks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ControlsLocked;

    /// <summary>
    /// Whether the tram requires power to operate.
    /// </summary>
    [DataField]
    public bool RequiresPower = true;

    /// <summary>
    /// If true, safety protocols are disabled (syndicate remote control).
    /// Ignores door states and obstacles.
    /// </summary>
    [DataField]
    public bool OverdriveMode;

    /// <summary>
    /// Entity UIDs of doors that should be synced with tram movement.
    /// </summary>
    [DataField]
    public List<EntityUid> LinkedDoors = new();
}

[Serializable, NetSerializable]
public enum TramVisuals : byte
{
    /// <summary>
    /// Visual state for the tram car.
    /// </summary>
    Moving,
}
