using System.Numerics;
using System.Linq; //DS14
using Content.Server.UserInterface;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.PowerCell;
using Content.Shared.Movement.Components;
// DS14-start
using Content.Shared.DeadSpace.Shuttles.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Reflection;
// DS14-end
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Shuttles.Systems;

public sealed class RadarConsoleSystem : SharedRadarConsoleSystem
{
    [Dependency] private readonly ShuttleConsoleSystem _console = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    // DS14-start
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xformSys = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    private static readonly Color FallbackColor = new Color(1f, 1f, 0f); // жёлтый — если AllowedComponents пуст

    private const float BlipRadius = 0.5f;

    private float _updateAccumulator;
    private const float UpdateInterval = 0.5f;
    // DS14-end

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadarConsoleComponent, ComponentStartup>(OnRadarStartup);
    }

    private void OnRadarStartup(EntityUid uid, RadarConsoleComponent component, ComponentStartup args)
    {
        UpdateState(uid, component);
    }

    // DS14-start
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateAccumulator += frameTime;
        if (_updateAccumulator < UpdateInterval)
            return;

        _updateAccumulator = 0f;

        var query = EntityQueryEnumerator<RadarConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_uiSystem.IsUiOpen(uid, RadarConsoleUiKey.Key))
                continue;

            UpdateState(uid, comp);
        }
    }
    // DS14-end

    protected override void UpdateState(EntityUid uid, RadarConsoleComponent component)
    {
        var xform = Transform(uid);
        var onGrid = xform.ParentUid == xform.GridUid;
        EntityCoordinates? coordinates = onGrid ? xform.Coordinates : null;
        Angle? angle = onGrid ? xform.LocalRotation : null;

        if (component.FollowEntity)
        {
            coordinates = new EntityCoordinates(uid, Vector2.Zero);
            angle = Angle.Zero;
        }

        // DS14-start
        if (!_uiSystem.HasUi(uid, RadarConsoleUiKey.Key))
            return;

        NavInterfaceState state;
        var docks = _console.GetAllDocks();

        if (coordinates != null && angle != null)
            state = _console.GetNavState(uid, docks, coordinates.Value, angle.Value);
        else
            state = _console.GetNavState(uid, docks);

        state.RotateWithEntity = !component.FollowEntity;

        if (component.Advanced)
            state.Blips = CollectSpaceBlips(uid, component);

        _uiSystem.SetUiState(uid, RadarConsoleUiKey.Key, new NavBoundUserInterfaceState(state));
    }

    private List<BlipState> CollectSpaceBlips(EntityUid consoleUid, RadarConsoleComponent component)
    {
        var blips = new List<BlipState>();

        var consoleXform = Transform(consoleUid);
        if (consoleXform.MapUid == null)
            return blips;

        var worldPos = _xformSys.GetWorldPosition(consoleXform);
        var mapId    = consoleXform.MapID;
        var blacklistTypes = ResolveComponentTypes(component.BlacklistComponents);
        var allowedTypes   = ResolveAllowedEntries(component.AllowedComponents);

        var nearby = new HashSet<EntityUid>();
        _lookup.GetEntitiesInRange(mapId, worldPos, component.MaxRange, nearby, LookupFlags.Uncontained);

        foreach (var ent in nearby)
        {
            if (ent == consoleUid)
                continue;

            if (HasComp<MapComponent>(ent) || HasComp<MapGridComponent>(ent))
                continue;

            var entXform = Transform(ent);
            if (entXform.GridUid != null)
                continue;

            if (!TryComp<PhysicsComponent>(ent, out var phys) || !phys.CanCollide)
                continue;

            if (blacklistTypes.Any(type => EntityManager.HasComponent(ent, type)))
                continue;

            var color = PickColor(ent, allowedTypes);

            if (color == null)
                continue;

            var entWorldPos = _xformSys.GetWorldPosition(entXform);
            blips.Add(new BlipState(entWorldPos, color.Value, BlipRadius));
        }

        return blips;
    }

    private Color? PickColor(EntityUid ent, List<(Type Type, Color Color)> allowedTypes)
    {
        if (allowedTypes.Count == 0)
            return FallbackColor;

        foreach (var (type, color) in allowedTypes)
        {
            if (EntityManager.HasComponent(ent, type))
                return color;
        }

        return null;
    }

    private List<Type> ResolveComponentTypes(List<string> names)
    {
        var result = new List<Type>(names.Count);
        foreach (var name in names)
        {
            if (_componentFactory.TryGetRegistration(name, out var reg))
                result.Add(reg.Type);
        }
        return result;
    }

    private List<(Type Type, Color Color)> ResolveAllowedEntries(List<RadarBlipEntry> entries)
    {
        var result = new List<(Type, Color)>(entries.Count);
        foreach (var entry in entries)
        {
            if (_componentFactory.TryGetRegistration(entry.Component, out var reg))
                result.Add((reg.Type, entry.Color));
        }
        return result;
    }
    // DS14-end
}