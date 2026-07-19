namespace Content.Shared.Tram.Components;

/// <summary>
/// Marks an entity as a tram station/stop.
/// Each station has a unique destination ID.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedTramSystem))]
public sealed partial class TramStationComponent : Component
{
    /// <summary>
    /// Unique identifier for this station destination.
    /// </summary>
    [DataField(required: true)]
    public string StationId = string.Empty;

    /// <summary>
    /// Display name shown in the console UI.
    /// </summary>
    [DataField]
    public string StationName = string.Empty;

    /// <summary>
    /// Icons to show in the TGUI (department icons).
    /// </summary>
    [DataField]
    public Dictionary<string, string> StationIcons = new();

    /// <summary>
    /// Doors that should be opened/closed when the tram arrives/departs.
    /// </summary>
    [DataField]
    public List<EntityUid> PlatformDoors = new();
}
