using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Tram;
using Content.Shared.Tram.Components;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Server.Tram.Systems;

/// <summary>
/// Server-side tram engine system. Handles the physical movement of the tram grid
/// along tracks, collision detection, door synchronization, and power management.
/// </summary>
public sealed class TramEngineSystem : SharedTramSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<DoorBoltComponent> _boltQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;

    /// <summary>
    /// Timer accumulator for tram movement ticks.
    /// </summary>
    private readonly Dictionary<EntityUid, TimeSpan> _moveTimers = new();

    public override void Initialize()
    {
        base.Initialize();

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _boltQuery = GetEntityQuery<DoorBoltComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<TramCarComponent, ComponentInit>(OnTramInit);
        SubscribeLocalEvent<TramCarComponent, ComponentStartup>(OnTramStartup);
        SubscribeLocalEvent<TramCarComponent, ComponentShutdown>(OnTramShutdown);

        SubscribeLocalEvent<TramConsoleComponent, ComponentInit>(OnConsoleInit);
        SubscribeLocalEvent<TramConsoleComponent, ComponentStartup>(OnConsoleStartup);
        SubscribeLocalEvent<TramConsoleComponent, AfterActivatableUIOpenEvent>(OnConsoleOpened);

        Subs.BuiEvents<TramConsoleComponent>(TramConsoleUiKey.Key, subs =>
        {
            subs.Event<TramConsoleSelectDestinationMessage>(OnDestinationSelected);
        });

        SubscribeLocalEvent<TramRemoteControlComponent, ComponentInit>(OnRemoteInit);
        SubscribeLocalEvent<TramRemoteControlComponent, AfterActivatableUIOpenEvent>(OnRemoteActivated);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = AllEntityQuery<TramCarComponent>();
        while (query.MoveNext(out var uid, out var tram))
        {
            if (!tram.Travelling || tram.TravelDistance <= 0)
                continue;

            // Accumulate time
            if (!_moveTimers.TryGetValue(uid, out var lastMove))
            {
                lastMove = _timing.CurTime;
                _moveTimers[uid] = lastMove;
            }

            var elapsed = _timing.CurTime - lastMove;
            if (elapsed.TotalSeconds < tram.MoveInterval)
                continue;

            _moveTimers[uid] = _timing.CurTime;

            // Execute one movement step
            ExecuteMoveStep(uid, tram);
        }
    }

    private void OnTramInit(EntityUid uid, TramCarComponent component, ComponentInit args)
    {
    }

    private void OnTramStartup(EntityUid uid, TramCarComponent component, ComponentStartup args)
    {
        UpdateTramLocation(uid, component);
    }

    private void OnTramShutdown(EntityUid uid, TramCarComponent component, ComponentShutdown args)
    {
        _moveTimers.Remove(uid);
    }

    private void OnConsoleInit(EntityUid uid, TramConsoleComponent component, ComponentInit args)
    {
    }

    private void OnConsoleStartup(EntityUid uid, TramConsoleComponent component, ComponentStartup args)
    {
    }

    private void OnConsoleOpened(EntityUid uid, TramConsoleComponent component, AfterActivatableUIOpenEvent args)
    {
        if (component.TramCar == null || !Exists(component.TramCar.Value))
        {
            var spawned = SpawnTramCar(uid);
            if (spawned != null)
                component.TramCar = spawned.Value;
        }

        UpdateConsoleState(uid, component);
    }

    private void OnDestinationSelected(EntityUid uid, TramConsoleComponent component, TramConsoleSelectDestinationMessage args)
    {
        if (component.TramCar == null || !TryComp<TramCarComponent>(component.TramCar.Value, out var tram))
            return;

        if (tram.ControlsLocked || tram.Travelling)
            return;

        var station = FindStationById(args.DestinationId);
        if (station == null)
            return;

        if (string.IsNullOrEmpty(tram.CurrentStation) || tram.CurrentStation == args.DestinationId)
            return;

        StartTravel(component.TramCar.Value, tram, args.DestinationId);
    }

    private void OnRemoteInit(EntityUid uid, TramRemoteControlComponent component, ComponentInit args)
    {
        // Try to find tram car on the same grid
        if (_xformQuery.TryComp(uid, out var xform) && xform.GridUid is { } gridUid)
        {
            var tramUid = FindTramCarOnGrid(gridUid);
            if (tramUid != null)
                component.LinkedTram = tramUid;
        }
    }

    private void OnRemoteActivated(EntityUid uid, TramRemoteControlComponent component, AfterActivatableUIOpenEvent args)
    {
        if (component.LinkedTram == null || !TryComp<TramCarComponent>(component.LinkedTram.Value, out var tram))
        {
            _popup.PopupEntity("No tram detected nearby.", uid, args.User);
            return;
        }

        component.OverdriveEnabled = !component.OverdriveEnabled;
        tram.OverdriveMode = component.OverdriveEnabled;
        Dirty(uid, component);
        Dirty(component.LinkedTram.Value, tram);

        var msg = component.OverdriveEnabled
            ? "Overdrive mode ENABLED. Safety protocols DISABLED. Doors will not close before departure."
            : "Overdrive mode disabled. Safety protocols restored.";
        _popup.PopupEntity(msg, uid, args.User);
    }

    /// <summary>
    /// Initiates travel from the current station to a target destination.
    /// </summary>
    public void StartTravel(EntityUid tramUid, TramCarComponent tram, string targetStationId)
    {
        if (string.IsNullOrEmpty(tram.CurrentStation))
            return;

        var fromStationId = tram.CurrentStation;

        SetControlsLocked((tramUid, tram), true);
        tram.Travelling = true;
        tram.TargetDestination = targetStationId;

        var fromStation = FindStationById(fromStationId);
        var toStation = FindStationById(targetStationId);
        if (fromStation == null || toStation == null)
        {
            StopTram(tramUid, tram);
            return;
        }

        var fromPos = TransformSystem.GetWorldPosition(fromStation.Value.Owner);
        var toPos = TransformSystem.GetWorldPosition(toStation.Value.Owner);

        var diff = toPos - fromPos;
        tram.TravelDirection = GetDirectionFromVector(diff);
        tram.TravelDistance = (int)(Math.Abs(diff.X) + Math.Abs(diff.Y));

        if (tram.TravelDistance <= 0)
        {
            StopTram(tramUid, tram);
            return;
        }

        if (!tram.OverdriveMode)
        {
            LockPlatformDoors(fromStation.Value, true);
        }

        var toName = toStation.Value.Comp.StationName;
        _popup.PopupEntity($"The tram is departing towards {toName}.", tramUid);

        var ev = new TramTravelStartedEvent(tramUid, fromStationId, targetStationId);
        RaiseLocalEvent(tramUid, ref ev);

        UpdateAllConsoleStates();
        _moveTimers[tramUid] = _timing.CurTime;
    }

    /// <summary>
    /// Executes a single movement step (one tile).
    /// </summary>
    private void ExecuteMoveStep(EntityUid tramUid, TramCarComponent tram)
    {
        if (tram.TravelDistance <= 0)
        {
            ArriveAtDestination(tramUid, tram);
            return;
        }

        if (!_xformQuery.TryComp(tramUid, out var xform))
            return;

        var currentPos = TransformSystem.GetWorldPosition(xform);
        var moveDir = DirectionToVector(tram.TravelDirection);
        var nextPos = currentPos + moveDir;

        // Check for obstacles on the track ahead
        if (!tram.OverdriveMode && HasObstacleAtPosition(nextPos))
        {
            _popup.PopupEntity("EMERGENCY: Obstacle detected on tracks! Tram stopped.", tramUid);
            StopTram(tramUid, tram);
            return;
        }

        // Move the entire grid
        TransformSystem.SetWorldPosition(xform, nextPos);

        // Check for collisions with entities on the track
        CheckCollisions(tramUid, tram, nextPos);

        tram.TravelDistance--;

        if (tram.TravelDistance <= 0)
        {
            ArriveAtDestination(tramUid, tram);
        }

        Dirty(tramUid, tram);
    }

    /// <summary>
    /// Called when the tram arrives at its destination.
    /// </summary>
    private void ArriveAtDestination(EntityUid tramUid, TramCarComponent tram)
    {
        if (string.IsNullOrEmpty(tram.TargetDestination))
        {
            StopTram(tramUid, tram);
            return;
        }

        tram.CurrentStation = tram.TargetDestination;
        tram.TargetDestination = null;
        tram.TravelDistance = 0;
        tram.Travelling = false;
        tram.TravelDirection = Direction.Invalid;

        var station = FindStationById(tram.CurrentStation);
        if (station != null && !tram.OverdriveMode)
        {
            LockPlatformDoors(station.Value, false);
        }

        // Unlock controls after a short delay
        Timer.Spawn(TimeSpan.FromSeconds(3), () =>
        {
            if (Deleted(tramUid))
                return;

            SetControlsLocked((tramUid, tram), false);
            UpdateAllConsoleStates();
        });

        var stationName = station?.Comp.StationName ?? "Unknown";
        _popup.PopupEntity($"The tram has arrived at {stationName}.", tramUid);

        var ev = new TramArrivedEvent(tramUid, tram.CurrentStation);
        RaiseLocalEvent(tramUid, ref ev);

        _moveTimers.Remove(tramUid);
        UpdateAllConsoleStates();
    }

    /// <summary>
    /// Emergency stop the tram.
    /// </summary>
    private void StopTram(EntityUid tramUid, TramCarComponent tram)
    {
        tram.Travelling = false;
        tram.TravelDistance = 0;
        tram.TravelDirection = Direction.Invalid;

        SetControlsLocked((tramUid, tram), false);
        _moveTimers.Remove(tramUid);
        UpdateAllConsoleStates();
    }

    /// <summary>
    /// Checks for entities on the track and applies collision damage.
    /// </summary>
    private void CheckCollisions(EntityUid tramUid, TramCarComponent tram, Vector2 position)
    {
        var entities = GetEntitiesAtPosition(position);

        foreach (var entity in entities)
        {
            if (entity == tramUid)
                continue;

            if (_xformQuery.TryComp(entity, out var exform) && exform.GridUid == tramUid)
                continue;

            var ev = new TramCollisionEvent(tramUid, entity);
            RaiseLocalEvent(tramUid, ref ev);

            if (_mobStateQuery.TryComp(entity, out var mobState))
            {
                ApplyTramDamage(entity);
            }
        }
    }

    /// <summary>
    /// Applies massive blunt damage from tram collision.
    /// </summary>
    private void ApplyTramDamage(EntityUid uid)
    {
        var damageSpec = new DamageSpecifier();
        damageSpec.DamageDict["Blunt"] = 200;

        _damageable.TryChangeDamage(uid, damageSpec);

        if (Deleted(uid))
            return;

        _popup.PopupEntity("You are hit by the tram!", uid);
    }

    /// <summary>
    /// Checks if there's an obstacle at the given position that should stop the tram.
    /// </summary>
    private bool HasObstacleAtPosition(Vector2 position)
    {
        var entities = GetEntitiesAtPosition(position);

        foreach (var entity in entities)
        {
            if (_boltQuery.TryComp(entity, out var bolt) && bolt.BoltsDown)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Locks or unlocks platform doors at a station.
    /// </summary>
    private void LockPlatformDoors(Entity<TramStationComponent> station, bool locked)
    {
        foreach (var doorUid in station.Comp.PlatformDoors)
        {
            if (!_boltQuery.TryComp(doorUid, out var bolt))
                continue;

            _door.SetBoltsDown((doorUid, bolt), locked);
        }
    }

    /// <summary>
    /// Gets all entities at a specific world position.
    /// </summary>
    private HashSet<EntityUid> GetEntitiesAtPosition(Vector2 position)
    {
        var result = new HashSet<EntityUid>();
        var query = AllEntityQuery<TransformComponent>();
        while (query.MoveNext(out var uid, out var xform))
        {
            var entPos = TransformSystem.GetWorldPosition(xform);
            if (Vector2.Distance(entPos, position) < 0.5f)
            {
                result.Add(uid);
            }
        }
        return result;
    }

    /// <summary>
    /// Converts a direction enum to a unit vector.
    /// </summary>
    private static Vector2 DirectionToVector(Direction direction)
    {
        return direction switch
        {
            Direction.North => new Vector2(0, 1),
            Direction.South => new Vector2(0, -1),
            Direction.East => new Vector2(1, 0),
            Direction.West => new Vector2(-1, 0),
            Direction.NorthEast => Vector2.Normalize(new Vector2(1, 1)),
            Direction.NorthWest => Vector2.Normalize(new Vector2(-1, 1)),
            Direction.SouthEast => Vector2.Normalize(new Vector2(1, -1)),
            Direction.SouthWest => Vector2.Normalize(new Vector2(-1, -1)),
            _ => Vector2.Zero,
        };
    }

    /// <summary>
    /// Gets the best direction from a vector.
    /// </summary>
    private static Direction GetDirectionFromVector(Vector2 vector)
    {
        if (Math.Abs(vector.X) > Math.Abs(vector.Y))
            return vector.X > 0 ? Direction.East : Direction.West;
        else
            return vector.Y > 0 ? Direction.North : Direction.South;
    }

    /// <summary>
    /// Updates the tram's current station based on its position.
    /// </summary>
    private void UpdateTramLocation(EntityUid tramUid, TramCarComponent tram)
    {
        if (!_xformQuery.TryComp(tramUid, out var xform))
            return;

        var tramPos = TransformSystem.GetWorldPosition(xform);
        string? closestStation = null;
        var closestDist = float.MaxValue;

        foreach (var (stationUid, stationComp) in FindAllStations())
        {
            var stationPos = TransformSystem.GetWorldPosition(stationUid);
            var dist = Vector2.Distance(tramPos, stationPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestStation = stationComp.StationId;
            }
        }

        tram.CurrentStation = closestStation;
        Dirty(tramUid, tram);
    }

    /// <summary>
    /// Spawns a simple tram car grid programmatically at the position of the first TramTrack entity.
    /// The tram car is a small platform grid that can be moved as a whole.
    /// </summary>
    private EntityUid? SpawnTramCar(EntityUid consoleUid)
    {
        if (!_xformQuery.TryComp(consoleUid, out var consoleXform))
            return null;

        var mapId = consoleXform.MapID;
        if (mapId == MapId.Nullspace)
            return null;

        EntityUid? firstTrack = null;
        var trackQuery = AllEntityQuery<TramTrackComponent, TransformComponent>();
        while (trackQuery.MoveNext(out var trackUid, out _, out var trackXform))
        {
            if (trackXform.MapID == mapId)
            {
                firstTrack = trackUid;
                break;
            }
        }

        if (firstTrack == null)
        {
            _popup.PopupEntity("No tram tracks found on this map!", consoleUid);
            return null;
        }

        var trackPos = TransformSystem.GetWorldPosition(firstTrack.Value);

        // Create a small grid for the tram car
        var (gridUid, gridComp) = _mapManager.CreateGridEntity(mapId);

        // Build a 3x5 platform
        var tileList = new List<(Vector2i, Tile)>();
        var floor = new Tile(2); // FloorTile = 2
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -2; y <= 2; y++)
            {
                tileList.Add((new Vector2i(x, y), floor));
            }
        }
        _mapSystem.SetTiles(gridUid, gridComp, tileList);

        // Position the grid at the track location
        TransformSystem.SetWorldPosition(gridUid, trackPos);

        // Name it for debugging
        _meta.SetEntityName(gridUid, "Debug Tram Car");

        // Add tram car component
        var tramComp = EnsureComp<TramCarComponent>(gridUid);
        UpdateTramLocation(gridUid, tramComp);

        Log.Info($"Tram car grid spawned at {trackPos} on map {mapId}");
        _popup.PopupEntity("Tram car spawned!", consoleUid);
        return gridUid;
    }

    /// <summary>
    /// Updates the console's BUI state.
    /// </summary>
    private void UpdateConsoleState(EntityUid consoleUid, TramConsoleComponent console)
    {
        if (console.TramCar == null || !TryComp<TramCarComponent>(console.TramCar.Value, out var tram))
        {
            var brokenState = new TramConsoleBoundUserInterfaceState
            {
                IsMoving = false,
                IsBroken = true,
                CurrentLocation = null,
                Destinations = new(),
                ControlsLocked = false,
            };
            _ui.SetUiState(consoleUid, TramConsoleUiKey.Key, brokenState);
            return;
        }

        var destinations = new List<TramDestinationInfo>();
        foreach (var (_, stationComp) in FindAllStations())
        {
            destinations.Add(new TramDestinationInfo
            {
                Id = stationComp.StationId,
                Name = stationComp.StationName,
                Icons = stationComp.StationIcons,
                IsHere = tram.CurrentStation == stationComp.StationId,
            });
        }

        var state = new TramConsoleBoundUserInterfaceState
        {
            IsMoving = tram.Travelling,
            IsBroken = false,
            CurrentLocation = tram.CurrentStation,
            Destinations = destinations,
            ControlsLocked = tram.ControlsLocked,
        };

        _ui.SetUiState(consoleUid, TramConsoleUiKey.Key, state);
    }

    /// <summary>
    /// Updates all console states (called when tram state changes).
    /// </summary>
    private void UpdateAllConsoleStates()
    {
        var query = AllEntityQuery<TramConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            UpdateConsoleState(uid, comp);
        }
    }
}
