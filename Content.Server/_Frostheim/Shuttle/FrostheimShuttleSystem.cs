using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._Frostheim.Shuttle;

namespace Content.Server._Frostheim.Shuttle;

public sealed class FrostheimShuttleSystem : EntitySystem
{
    [Dependency] private readonly ShuttleSystem _shuttleSystem = default!;

    private readonly HashSet<EntityUid> _pendingInit = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostheimShuttleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AnchorStateChangedEvent>(OnAnchorChanged);
    }

    private void OnMapInit(EntityUid uid, FrostheimShuttleComponent shuttle, MapInitEvent args)
    {
        var gridUid = Transform(uid).GridUid;
        if (gridUid.HasValue)
            _shuttleSystem.Disable(gridUid.Value);

        _pendingInit.Add(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var uid in _pendingInit)
        {
            if (!TryComp<FrostheimShuttleComponent>(uid, out var shuttle))
                continue;

            CountThrusters(uid, out var linear, out var angular);
            shuttle.InitialThrusters = linear;
            shuttle.CurrentThrusters = linear;
            shuttle.HasGyroscope = angular;
            shuttle.Initialized = true;
            UpdateReady(uid, shuttle);
        }

        _pendingInit.Clear();
    }

    private void OnAnchorChanged(ref AnchorStateChangedEvent args)
    {
        if (!HasComp<ThrusterComponent>(args.Entity))
            return;

        var gridUid = Transform(args.Entity).GridUid;
        if (gridUid == null || !TryComp<FrostheimShuttleComponent>(gridUid, out var shuttle))
            return;

        if (!shuttle.Initialized)
            return;

        CountThrusters(gridUid.Value, out var linear, out var angular);
        shuttle.CurrentThrusters = linear;
        shuttle.HasGyroscope = angular;
        UpdateReady(gridUid.Value, shuttle);
    }

    private void CountThrusters(EntityUid gridUid, out int linear, out bool angular)
    {
        linear = 0;
        angular = false;

        var query = EntityQueryEnumerator<ThrusterComponent, TransformComponent>();
        while (query.MoveNext(out _, out var thruster, out var xform))
        {
            if (xform.GridUid != gridUid || !xform.Anchored)
                continue;

            if (thruster.Type == ThrusterType.Linear)
                linear++;
            else if (thruster.Type == ThrusterType.Angular)
                angular = true;
        }
    }

    public void UpdateReady(EntityUid uid, FrostheimShuttleComponent shuttle)
    {
        var needed = shuttle.InitialThrusters + shuttle.AdditionalThrustersNeeded;
        var ready = shuttle.CurrentThrusters >= needed
                    && (!shuttle.RequireGyroscope || shuttle.HasGyroscope)
                    && shuttle.NavigationComplete;

        shuttle.Ready = ready;
        Dirty(uid, shuttle);
    }
}
