namespace Content.Shared.Tram.Components;

/// <summary>
/// Marks an entity as a tram track piece.
/// Defines the direction the tram can travel on this tile.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedTramSystem))]
public sealed partial class TramTrackComponent : Component
{
    /// <summary>
    /// The direction(s) this track allows travel along.
    /// </summary>
    [DataField]
    public Direction[] AllowedDirections = { Direction.East, Direction.West };
}
