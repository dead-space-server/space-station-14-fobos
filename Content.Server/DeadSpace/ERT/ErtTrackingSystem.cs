// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Content.Server.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Database;
using Content.Shared.DeadSpace.ERT.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Pinpointer;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.ERT;

public sealed class ErtTrackingSystem : EntitySystem
{
    private static readonly ProtoId<AlertPrototype> TrackingAlert = "ErtTracking";

    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IdCardSystem _idCard = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<TransformComponent> _transformQuery;

    public override void Initialize()
    {
        base.Initialize();

        _transformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<ErtTrackerPdaComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<ErtTrackingComponent, ComponentShutdown>(OnTrackingShutdown);
    }

    private void OnAfterInteract(Entity<ErtTrackerPdaComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not { } target)
            return;

        if (!args.CanReach)
        {
            _popup.PopupEntity(Loc.GetString("ert-tracking-too-far"), args.User, args.User, PopupType.SmallCaution);
            args.Handled = true;
            return;
        }

        if (!HasComp<MobStateComponent>(target) || !_transformQuery.HasComp(target))
        {
            _popup.PopupEntity(Loc.GetString("ert-tracking-invalid-target"), args.User, args.User, PopupType.SmallCaution);
            args.Handled = true;
            return;
        }

        var tracking = EnsureComp<ErtTrackingComponent>(args.User);
        SetTarget(args.User, target, tracking);

        var targetName = tracking.TargetName ?? Identity.Name(target, EntityManager);
        _popup.PopupEntity(Loc.GetString("ert-tracking-set", ("target", targetName)), args.User, args.User);
        _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):user} set ERT tracking target to {ToPrettyString(target):target} with {ToPrettyString(ent.Owner):pda}");

        args.Handled = true;
    }

    private void OnTrackingShutdown(Entity<ErtTrackingComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent.Owner, TrackingAlert);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ErtTrackingComponent>();
        while (query.MoveNext(out var uid, out var tracking))
        {
            UpdateDirectionToTarget(uid, tracking);
        }
    }

    private void SetTarget(EntityUid user, EntityUid target, ErtTrackingComponent tracking)
    {
        tracking.Target = target;
        tracking.TargetName = Identity.Name(target, EntityManager);
        tracking.TargetJobName = GetTargetJobName(target);
        Dirty(user, tracking);

        UpdateDirectionToTarget(user, tracking);
    }

    private string GetTargetJobName(EntityUid target)
    {
        if (_idCard.TryFindIdCard(target, out var idCard))
        {
            var jobName = idCard.Comp.LocalizedJobTitle;
            if (!string.IsNullOrWhiteSpace(jobName))
                return jobName;
        }

        return Loc.GetString("generic-unknown-title");
    }

    private void UpdateDirectionToTarget(EntityUid uid, ErtTrackingComponent tracking)
    {
        if (tracking.Target is not { } target || !Exists(target))
        {
            SetDistance(uid, Distance.Unknown, tracking);
            return;
        }

        var direction = CalculateDirection(uid, target);
        if (direction == null)
        {
            SetDistance(uid, Distance.Unknown, tracking);
            return;
        }

        TrySetArrowAngle(uid, direction.Value.ToWorldAngle(), tracking);
        SetDistance(uid, CalculateDistance(direction.Value, tracking), tracking);
    }

    private Vector2? CalculateDirection(EntityUid source, EntityUid target)
    {
        if (!_transformQuery.TryGetComponent(source, out var sourceTransform))
            return null;

        if (!_transformQuery.TryGetComponent(target, out var targetTransform))
            return null;

        if (sourceTransform.MapID != targetTransform.MapID)
            return null;

        return _transform.GetWorldPosition(targetTransform, _transformQuery) -
               _transform.GetWorldPosition(sourceTransform, _transformQuery);
    }

    private Distance CalculateDistance(Vector2 direction, ErtTrackingComponent tracking)
    {
        var distance = direction.Length();

        if (distance <= tracking.ReachedDistance)
            return Distance.Reached;

        if (distance <= tracking.CloseDistance)
            return Distance.Close;

        if (distance <= tracking.MediumDistance)
            return Distance.Medium;

        return Distance.Far;
    }

    private void SetDistance(EntityUid uid, Distance distance, ErtTrackingComponent tracking)
    {
        if (tracking.DistanceToTarget == distance)
        {
            if (distance == Distance.Unknown)
                _alerts.ClearAlert(uid, TrackingAlert);

            return;
        }

        tracking.DistanceToTarget = distance;
        Dirty(uid, tracking);

        if (distance == Distance.Unknown)
        {
            _alerts.ClearAlert(uid, TrackingAlert);
            return;
        }

        _alerts.ShowAlert(uid, TrackingAlert, (short) distance);
    }

    private void TrySetArrowAngle(EntityUid uid, Angle arrowAngle, ErtTrackingComponent tracking)
    {
        if (tracking.ArrowAngle.EqualsApprox(arrowAngle, tracking.Precision))
            return;

        tracking.ArrowAngle = arrowAngle;
        Dirty(uid, tracking);
    }
}
