using Content.Server.Shuttles.Components;
using Content.Shared.PipeShuttle;
using Content.Shared.PipeShuttle.Components;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Server.PipeShuttle.Systems;

public sealed class PipeShuttleSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;

    private readonly Dictionary<EntityUid, TimeSpan> _cooldowns = new();

    public override void Initialize()
    {
        base.Initialize();

        _xformQuery = GetEntityQuery<TransformComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<PipeShuttleComponent, MapInitEvent>(OnShuttleMapInit);

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
            EnforcePhysics(uid);

            if (!shuttle.Travelling || string.IsNullOrEmpty(shuttle.TargetDestId))
                continue;

            var dest = FindDestination(shuttle, shuttle.TargetDestId);
            if (dest == null)
            {
                CancelShuttle(uid, shuttle);
                continue;
            }

            var currentPos = _transform.GetWorldPosition(xform);
            var diff = dest.Position - currentPos;
            var dist = diff.Length();

            if (dist < shuttle.ArrivalThreshold)
            {
                ArriveAtDestination(uid, shuttle, dest);
                continue;
            }

            _transform.SetWorldPosition(xform, currentPos + diff * MathF.Min(shuttle.MoveSpeed * frameTime / dist, 1f));
        }
    }

    private void EnforcePhysics(EntityUid uid)
    {
        if (!_physicsQuery.TryComp(uid, out var body))
            return;

        if (body.BodyType != BodyType.Dynamic)
            _physics.SetBodyType(uid, BodyType.Dynamic, body: body);

        if (!body.FixedRotation)
            _physics.SetFixedRotation(uid, true, body: body);

        if (body.CanCollide)
            _physics.SetCanCollide(uid, false, body: body);

        if (body.LinearVelocity != Vector2.Zero)
            _physics.SetLinearVelocity(uid, Vector2.Zero, body: body);

        if (body.AngularVelocity != 0f)
            _physics.SetAngularVelocity(uid, 0f, body: body);
    }

    private void OnShuttleMapInit(EntityUid uid, PipeShuttleComponent component, MapInitEvent args)
    {
        RemComp<ShuttleComponent>(uid);
        EnforcePhysics(uid);
    }

    private void OnCallOpened(EntityUid uid, PipeShuttleCallComponent component, AfterActivatableUIOpenEvent args)
    {
        SendStateToAll();
    }

    private void OnCallMessage(EntityUid uid, PipeShuttleCallComponent component, PipeShuttleCallMessage args)
    {
        CallShuttleToDest(args.DestId, uid);
    }

    public void CallShuttleToDest(string targetDestId, EntityUid? callerUid = null)
    {
        EntityUid? shuttleUid = null;
        PipeShuttleComponent? shuttleComp = null;

        var shuttleQuery = AllEntityQuery<PipeShuttleComponent>();
        while (shuttleQuery.MoveNext(out var uid, out var comp))
        {
            shuttleUid = uid;
            shuttleComp = comp;
            break;
        }

        if (shuttleUid == null || shuttleComp == null)
        {
            _popup.PopupEntity("No pipe shuttle found!", callerUid ?? default);
            return;
        }

        if (shuttleComp.Travelling)
        {
            _popup.PopupEntity("Shuttle is already in transit!", callerUid ?? default);
            return;
        }

        if (shuttleComp.CurrentDestId == targetDestId)
        {
            _popup.PopupEntity("Shuttle is already here!", callerUid ?? default);
            return;
        }

        if (_cooldowns.TryGetValue(shuttleUid.Value, out var cooldownEnd) && _timing.CurTime < cooldownEnd)
        {
            var remaining = (cooldownEnd - _timing.CurTime).TotalSeconds;
            _popup.PopupEntity($"Wait {remaining:F0}s before calling shuttle again.", callerUid ?? default);
            return;
        }

        var dest = FindDestination(shuttleComp, targetDestId);
        if (dest == null)
        {
            _popup.PopupEntity("Invalid destination!", callerUid ?? default);
            return;
        }

        shuttleComp.TargetDestId = targetDestId;
        shuttleComp.Travelling = true;
        shuttleComp.CurrentDestId = null;
        Dirty(shuttleUid.Value, shuttleComp);

        _popup.PopupEntity($"Shuttle heading to {dest.Name}!", shuttleUid.Value);
        SendStateToAll();
    }

    private void ArriveAtDestination(EntityUid shuttleUid, PipeShuttleComponent shuttle, PipeShuttleDestination dest)
    {
        _transform.SetWorldPosition(shuttleUid, dest.Position);

        shuttle.TargetDestId = null;
        shuttle.Travelling = false;
        shuttle.CurrentDestId = dest.Id;
        Dirty(shuttleUid, shuttle);

        _popup.PopupEntity($"Shuttle arrived at {dest.Name}!", shuttleUid);
        _cooldowns[shuttleUid] = _timing.CurTime + TimeSpan.FromSeconds(shuttle.Cooldown);
        SendStateToAll();
    }

    private void CancelShuttle(EntityUid uid, PipeShuttleComponent shuttle)
    {
        shuttle.Travelling = false;
        shuttle.TargetDestId = null;
        Dirty(uid, shuttle);
        SendStateToAll();
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

    private void SendStateToAll()
    {
        EntityUid? shuttleUid = null;
        PipeShuttleComponent? shuttleComp = null;

        var shuttleQuery = AllEntityQuery<PipeShuttleComponent>();
        while (shuttleQuery.MoveNext(out var uid, out var comp))
        {
            shuttleUid = uid;
            shuttleComp = comp;
            break;
        }

        var dests = new List<PipeShuttleDestInfo>();
        if (shuttleComp != null)
        {
            foreach (var dest in shuttleComp.Destinations)
            {
                dests.Add(new PipeShuttleDestInfo
                {
                    Id = dest.Id,
                    Name = dest.Name,
                });
            }
        }

        var state = new PipeShuttleUiState
        {
            Destinations = dests,
            CurrentDestId = shuttleComp?.CurrentDestId,
            Travelling = shuttleComp?.Travelling ?? false,
            TargetDestId = shuttleComp?.TargetDestId,
        };

        var callerQuery = AllEntityQuery<PipeShuttleCallComponent>();
        while (callerQuery.MoveNext(out var uid, out _))
        {
            _ui.SetUiState(uid, PipeShuttleUiKey.Key, state);
        }
    }
}
