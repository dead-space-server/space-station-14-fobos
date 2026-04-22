using System.Numerics;
using Content.Server.UserInterface;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.PowerCell;
using Content.Shared.Movement.Components;
//DS14-start
using Content.Shared.Mobs.Components;
using Content.Shared.Mind;
using Content.Shared.DeadSpace.Shuttles.BUIStates;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
//DS14-end
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Shuttles.Systems;

public sealed class RadarConsoleSystem : SharedRadarConsoleSystem
{
    [Dependency] private readonly ShuttleConsoleSystem _console = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    //DS14-start
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xformSys = default!;

    // Белый  = сущность под управлением игрока (есть MindComponent)
    // Синий  = NPC-моб (есть MobStateComponent, но нет MindComponent)
    // Жёлтый = всё остальное (предметы, машины, обломки и т.п.)
    private static readonly Color PlayerColor = Color.White;
    private static readonly Color MobColor    = Color.CornflowerBlue;
    private static readonly Color MiscColor   = new Color(1f, 1f, 0f);

    private const float BlipRadius = 0.5f;

    private float _updateAccumulator;
    private const float UpdateInterval = 0.5f;
    //DS14-end

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadarConsoleComponent, ComponentStartup>(OnRadarStartup);
    }

    private void OnRadarStartup(EntityUid uid, RadarConsoleComponent component, ComponentStartup args)
    {
        UpdateState(uid, component);
    }

    //DS14-start
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
    //DS14-end

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

        if (!_uiSystem.HasUi(uid, RadarConsoleUiKey.Key))
            return;

        NavInterfaceState state;
        var docks = _console.GetAllDocks();

        if (coordinates != null && angle != null)
        {
            state = _console.GetNavState(uid, docks, coordinates.Value, angle.Value);
        }
        else
        {
            state = _console.GetNavState(uid, docks);
        }

        state.RotateWithEntity = !component.FollowEntity;

        //DS14: блипы показываются только если консоль помечена как advanced
        if (component.Advanced)
            state.Blips = CollectSpaceBlips(uid, component.MaxRange);

        _uiSystem.SetUiState(uid, RadarConsoleUiKey.Key, new NavBoundUserInterfaceState(state));
    }

    //DS14-start
    private List<BlipState> CollectSpaceBlips(EntityUid consoleUid, float maxRange)
    {
        var blips = new List<BlipState>();

        var consoleXform = Transform(consoleUid);
        if (consoleXform.MapUid == null)
            return blips;

        var worldPos = _xformSys.GetWorldPosition(consoleXform);
        var mapId    = consoleXform.MapID;

        var nearby = new HashSet<EntityUid>();
        _lookup.GetEntitiesInRange(mapId, worldPos, maxRange, nearby, LookupFlags.Uncontained);

        foreach (var ent in nearby)
        {
            if (ent == consoleUid)
                continue;

            // Пропускаем сами объекты карты/грида
            if (HasComp<MapComponent>(ent) || HasComp<MapGridComponent>(ent))
                continue;

            var entXform = Transform(ent);

            // Только сущности в открытом космосе (без родительского грида)
            if (entXform.GridUid != null)
                continue;

            // Должно быть твёрдое физическое тело
            if (!TryComp<PhysicsComponent>(ent, out var phys) || !phys.CanCollide)
                continue;

            var entWorldPos = _xformSys.GetWorldPosition(entXform);
            blips.Add(new BlipState(entWorldPos, PickColor(ent), BlipRadius));
        }

        return blips;
    }

    private Color PickColor(EntityUid ent)
    {
        if (HasComp<MindComponent>(ent))     return PlayerColor; // белый  — игрок
        if (HasComp<MobStateComponent>(ent)) return MobColor;    // синий  — NPC
        return MiscColor;                                         // жёлтый — предмет
    }
    //DS14-end
}