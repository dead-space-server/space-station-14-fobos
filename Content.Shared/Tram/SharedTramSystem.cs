using Content.Shared.Tram.Components;

namespace Content.Shared.Tram;

/// <summary>
/// Shared tram system. Handles state queries and shared logic.
/// Server-specific movement logic lives in TramEngineSystem.
/// </summary>
public abstract class SharedTramSystem : EntitySystem
{
    [Dependency] protected readonly SharedMapSystem MapSystem = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Finds all tram cars in the game.
    /// </summary>
    public IEnumerable<Entity<TramCarComponent>> FindAllTramCars()
    {
        var query = AllEntityQuery<TramCarComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            yield return (uid, comp);
        }
    }

    /// <summary>
    /// Finds all tram stations in the game.
    /// </summary>
    public IEnumerable<Entity<TramStationComponent>> FindAllStations()
    {
        var query = AllEntityQuery<TramStationComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            yield return (uid, comp);
        }
    }

    /// <summary>
    /// Finds the tram car entity on a specific grid.
    /// </summary>
    public EntityUid? FindTramCarOnGrid(EntityUid gridUid)
    {
        var query = AllEntityQuery<TramCarComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (xform.GridUid == gridUid)
                return uid;
        }
        return null;
    }

    /// <summary>
    /// Finds a station by its ID.
    /// </summary>
    public Entity<TramStationComponent>? FindStationById(string stationId)
    {
        var query = AllEntityQuery<TramStationComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.StationId == stationId)
                return (uid, comp);
        }
        return null;
    }

    /// <summary>
    /// Sets the controls locked state on the tram and raises the appropriate event.
    /// </summary>
    public void SetControlsLocked(Entity<TramCarComponent> tram, bool locked)
    {
        if (tram.Comp.ControlsLocked == locked)
            return;

        tram.Comp.ControlsLocked = locked;
        Dirty(tram, tram.Comp);

        var ev = new TramControlsLockChangedEvent(tram.Owner, locked);
        RaiseLocalEvent(tram.Owner, ref ev);
    }
}
