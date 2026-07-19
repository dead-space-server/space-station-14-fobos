namespace Content.Shared.Tram;

/// <summary>
/// Raised when the tram begins travelling to a destination.
/// </summary>
[ByRefEvent]
public readonly record struct TramTravelStartedEvent(EntityUid Tram, string FromStation, string ToStation);

/// <summary>
/// Raised when the tram arrives at a destination.
/// </summary>
[ByRefEvent]
public readonly record struct TramArrivedEvent(EntityUid Tram, string StationId);

/// <summary>
/// Raised when the tram collides with an entity on the track.
/// </summary>
[ByRefEvent]
public readonly record struct TramCollisionEvent(EntityUid Tram, EntityUid Collided);

/// <summary>
/// Raised when tram controls are locked or unlocked.
/// </summary>
[ByRefEvent]
public readonly record struct TramControlsLockChangedEvent(EntityUid Tram, bool Locked);

/// <summary>
/// Raised when overdrive mode is toggled on/off.
/// </summary>
[ByRefEvent]
public readonly record struct TramOverdriveToggledEvent(EntityUid Tram, bool Enabled);
