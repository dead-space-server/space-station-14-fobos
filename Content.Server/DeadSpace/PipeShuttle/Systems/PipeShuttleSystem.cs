// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Content.Server.Shuttles.Components;
using Content.Shared.DeadSpace.PipeShuttle;
using Content.Shared.DeadSpace.PipeShuttle.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.PipeShuttle.Systems;

public sealed class PipeShuttleSystem : EntitySystem
{
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    private readonly Dictionary<EntityUid, TimeSpan> _cooldowns = new();

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<PipeShuttleComponent, MapInitEvent>(OnShuttleMapInit);
        SubscribeLocalEvent<PipeShuttleComponent, ComponentShutdown>(OnShuttleShutdown);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        SubscribeLocalEvent<PipeShuttleCallComponent, AfterActivatableUIOpenEvent>(OnCallOpened);
        Subs.BuiEvents<PipeShuttleCallComponent>(PipeShuttleUiKey.Key, subs =>
        {
            subs.Event<PipeShuttleCallMessage>(OnCallMessage);
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var shuttleQuery = AllEntityQuery<PipeShuttleComponent, TransformComponent>();
        while (shuttleQuery.MoveNext(out var uid, out var shuttle, out var xform))
        {
            if (!shuttle.Travelling || string.IsNullOrEmpty(shuttle.TargetDestId))
                continue;

            var dest = FindDestination(shuttle, shuttle.TargetDestId);
            if (dest == null)
            {
                CancelShuttle(uid, shuttle);
                continue;
            }

            if (!shuttle.DoorsSecured)
            {
                var result = TrySecureDoors(uid);
                if (result == DoorSecureResult.Invalid)
                {
                    _popup.PopupEntity(Loc.GetString("pipe-shuttle-popup-doors-unavailable"), uid);
                    CancelShuttle(uid, shuttle);
                    continue;
                }

                if (result == DoorSecureResult.InProgress)
                    continue;

                shuttle.DoorsSecured = true;
                shuttle.CurrentDestId = null;
                Dirty(uid, shuttle);
                SendStateForShuttle(uid, shuttle);
            }

            var currentPos = _transform.GetWorldPosition(xform);
            var targetPos = dest.Position + shuttle.PositionOffset;
            var diff = targetPos - currentPos;
            var dist = diff.Length();

            if (dist < shuttle.ArrivalThreshold)
            {
                ArriveAtDestination(uid, shuttle, dest);
                continue;
            }

            _transform.SetWorldPosition(xform, currentPos + diff * MathF.Min(shuttle.MoveSpeed * frameTime / dist, 1f));
        }
    }

    private void OnShuttleMapInit(EntityUid uid, PipeShuttleComponent component, MapInitEvent args)
    {
        RemComp<ShuttleComponent>(uid);
        component.DoorsSecured = false;

        if (_physicsQuery.TryComp(uid, out var body))
        {
            _physics.SetBodyType(uid, BodyType.Static, body: body);
            _physics.SetFixedRotation(uid, true, body: body);
            _physics.SetCanCollide(uid, false, body: body);
        }

        if (string.IsNullOrEmpty(component.CurrentDestId))
            return;

        var dest = FindDestination(component, component.CurrentDestId);
        if (dest != null)
        {
            var gridPos = _transform.GetWorldPosition(uid);
            component.PositionOffset = gridPos - dest.Position;
        }
    }

    private void OnShuttleShutdown(EntityUid uid, PipeShuttleComponent component, ComponentShutdown args)
    {
        _cooldowns.Remove(uid);

        if (!TerminatingOrDeleted(uid))
            ReleaseDoors(uid);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _cooldowns.Clear();
    }

    private void OnCallOpened(EntityUid uid, PipeShuttleCallComponent component, AfterActivatableUIOpenEvent args)
    {
        SendState((uid, component));
    }

    private void OnCallMessage(EntityUid uid, PipeShuttleCallComponent component, PipeShuttleCallMessage args)
    {
        TryCallShuttleToDest(args.DestId, (uid, component), args.Actor);
    }

    public bool TryCallShuttleToDest(
        string targetDestId,
        Entity<PipeShuttleCallComponent> call,
        EntityUid caller)
    {
        if (!TryGetBoundShuttle(call, out var shuttle))
        {
            PopupCaller(Loc.GetString("pipe-shuttle-popup-not-found"), call.Owner, caller);
            return false;
        }

        if (shuttle.Comp.Travelling)
        {
            PopupCaller(Loc.GetString("pipe-shuttle-popup-already-travelling"), call.Owner, caller);
            return false;
        }

        if (shuttle.Comp.CurrentDestId == targetDestId)
        {
            PopupCaller(Loc.GetString("pipe-shuttle-popup-already-here"), call.Owner, caller);
            return false;
        }

        if (_cooldowns.TryGetValue(shuttle.Owner, out var cooldownEnd) && _timing.CurTime < cooldownEnd)
        {
            var remaining = (int) Math.Ceiling((cooldownEnd - _timing.CurTime).TotalSeconds);
            PopupCaller(Loc.GetString("pipe-shuttle-popup-cooldown", ("seconds", remaining)), call.Owner, caller);
            return false;
        }

        var dest = FindDestination(shuttle.Comp, targetDestId);
        if (dest == null)
        {
            PopupCaller(Loc.GetString("pipe-shuttle-popup-invalid-destination"), call.Owner, caller);
            return false;
        }

        if (!HasManagedDoors(shuttle.Owner))
        {
            PopupCaller(Loc.GetString("pipe-shuttle-popup-doors-unavailable"), call.Owner, caller);
            return false;
        }

        shuttle.Comp.TargetDestId = targetDestId;
        shuttle.Comp.Travelling = true;
        shuttle.Comp.DoorsSecured = false;
        Dirty(shuttle);

        _popup.PopupEntity(
            Loc.GetString("pipe-shuttle-popup-departing", ("destination", Loc.GetString(dest.Name))),
            shuttle.Owner);
        SendStateForShuttle(shuttle);
        return true;
    }

    private void ArriveAtDestination(EntityUid shuttleUid, PipeShuttleComponent shuttle, PipeShuttleDestination dest)
    {
        _transform.SetWorldPosition(shuttleUid, dest.Position + shuttle.PositionOffset);

        shuttle.TargetDestId = null;
        shuttle.Travelling = false;
        shuttle.CurrentDestId = dest.Id;
        shuttle.DoorsSecured = false;
        Dirty(shuttleUid, shuttle);

        ReleaseDoors(shuttleUid);
        _popup.PopupEntity(
            Loc.GetString("pipe-shuttle-popup-arrived", ("destination", Loc.GetString(dest.Name))),
            shuttleUid);
        _cooldowns[shuttleUid] = _timing.CurTime + TimeSpan.FromSeconds(shuttle.Cooldown);
        SendStateForShuttle(shuttleUid, shuttle);
    }

    private void CancelShuttle(EntityUid uid, PipeShuttleComponent shuttle)
    {
        shuttle.Travelling = false;
        shuttle.TargetDestId = null;
        shuttle.DoorsSecured = false;
        Dirty(uid, shuttle);

        ReleaseDoors(uid);
        SendStateForShuttle(uid, shuttle);
    }

    private DoorSecureResult TrySecureDoors(EntityUid shuttleUid)
    {
        var foundDoor = false;
        var secured = true;
        var query = AllEntityQuery<DoorComponent, DoorBoltComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var door, out var bolt, out var xform))
        {
            if (xform.GridUid != shuttleUid)
                continue;

            foundDoor = true;

            if (door.State == DoorState.Welded)
                continue;

            if (door.State != DoorState.Closed)
            {
                secured = false;

                if (bolt.BoltsDown)
                {
                    _door.SetBoltsDown((uid, bolt), false);
                    continue;
                }

                if (door.State != DoorState.Closing)
                    _door.TryClose(uid, door);

                continue;
            }

            if (!bolt.BoltsDown)
                _door.SetBoltsDown((uid, bolt), true);

            if (!bolt.BoltsDown)
                secured = false;
        }

        if (!foundDoor)
            return DoorSecureResult.Invalid;

        return secured ? DoorSecureResult.Secured : DoorSecureResult.InProgress;
    }

    private bool HasManagedDoors(EntityUid shuttleUid)
    {
        var query = AllEntityQuery<DoorComponent, DoorBoltComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out _, out var xform))
        {
            if (xform.GridUid == shuttleUid)
                return true;
        }

        return false;
    }

    private void ReleaseDoors(EntityUid shuttleUid)
    {
        var query = AllEntityQuery<DoorComponent, DoorBoltComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var bolt, out var xform))
        {
            if (xform.GridUid != shuttleUid)
                continue;

            if (bolt.BoltsDown)
                _door.SetBoltsDown((uid, bolt), false);
        }
    }

    private bool TryGetBoundShuttle(
        Entity<PipeShuttleCallComponent> call,
        out Entity<PipeShuttleComponent> shuttle)
    {
        shuttle = default;

        if (call.Comp.Shuttle is not { } shuttleUid ||
            !TryComp<PipeShuttleComponent>(shuttleUid, out var shuttleComp) ||
            Transform(call.Owner).MapID != Transform(shuttleUid).MapID)
        {
            return false;
        }

        shuttle = (shuttleUid, shuttleComp);
        return true;
    }

    private void SendState(Entity<PipeShuttleCallComponent> call)
    {
        if (!TryGetBoundShuttle(call, out var shuttle))
        {
            _ui.SetUiState(call.Owner, PipeShuttleUiKey.Key, new PipeShuttleUiState());
            return;
        }

        _ui.SetUiState(call.Owner, PipeShuttleUiKey.Key, CreateState(shuttle.Comp));
    }

    private void SendStateForShuttle(Entity<PipeShuttleComponent> shuttle)
    {
        SendStateForShuttle(shuttle.Owner, shuttle.Comp);
    }

    private void SendStateForShuttle(EntityUid shuttleUid, PipeShuttleComponent shuttle)
    {
        var state = CreateState(shuttle);
        var callerQuery = AllEntityQuery<PipeShuttleCallComponent>();
        while (callerQuery.MoveNext(out var uid, out var call))
        {
            if (call.Shuttle != shuttleUid || Transform(uid).MapID != Transform(shuttleUid).MapID)
                continue;

            _ui.SetUiState(uid, PipeShuttleUiKey.Key, state);
        }
    }

    private static PipeShuttleUiState CreateState(PipeShuttleComponent shuttle)
    {
        var dests = new List<PipeShuttleDestInfo>();
        foreach (var dest in shuttle.Destinations)
        {
            dests.Add(new PipeShuttleDestInfo
            {
                Id = dest.Id,
                Name = dest.Name,
            });
        }

        return new PipeShuttleUiState
        {
            Destinations = dests,
            CurrentDestId = shuttle.CurrentDestId,
            Travelling = shuttle.Travelling,
            TargetDestId = shuttle.TargetDestId,
        };
    }

    private static PipeShuttleDestination? FindDestination(PipeShuttleComponent shuttle, string destId)
    {
        foreach (var dest in shuttle.Destinations)
        {
            if (dest.Id == destId)
                return dest;
        }

        return null;
    }

    private void PopupCaller(string message, EntityUid callUid, EntityUid caller)
    {
        _popup.PopupEntity(message, callUid, caller);
    }

    private enum DoorSecureResult : byte
    {
        Invalid,
        InProgress,
        Secured,
    }
}
