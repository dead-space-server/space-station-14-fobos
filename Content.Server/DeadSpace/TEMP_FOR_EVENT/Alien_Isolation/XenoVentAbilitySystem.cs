using System.Numerics;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.Popups;
using Content.Shared.DeadSpace.TEMP_FOR_EVENT.Alien_Isolation;
using Content.Shared.Movement.Components;
using Content.Shared.Physics;
using Content.Shared.Toggleable;
using Content.Shared.ActionBlocker;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Tag;
using Content.Shared.Tools.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;

namespace Content.Server.DeadSpace.TEMP_FOR_EVENT.Alien_Isolation;

public sealed class XenoVentAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoVentAbilityComponent, ToggleActionEvent>(OnToggled);
        SubscribeLocalEvent<XenoVentAbilityComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<XenoVentAbilityComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<XenoVentAbilityComponent, UseAttemptEvent>(OnUseAttempt);
        SubscribeLocalEvent<XenoVentAbilityComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<XenoVentAbilityComponent, PickupAttemptEvent>(OnPickupAttempt);
    }

    private void OnAttackAttempt(EntityUid uid, XenoVentAbilityComponent comp, AttackAttemptEvent args)
    {
        if (comp.IsActive)
            args.Cancel();
    }

    private void OnUseAttempt(EntityUid uid, XenoVentAbilityComponent comp, UseAttemptEvent args)
    {
        if (comp.IsActive)
            args.Cancel();
    }

    private void OnInteractAttempt(EntityUid uid, XenoVentAbilityComponent comp, InteractionAttemptEvent args)
    {
        if (comp.IsActive)
            args.Cancelled = true;
    }

    private void OnPickupAttempt(EntityUid uid, XenoVentAbilityComponent comp, PickupAttemptEvent args)
    {
        if (comp.IsActive)
            args.Cancel();
    }

    private const string LargeCrawlerTag = "LargeVentCrawler";

    private void OnToggled(EntityUid uid, XenoVentAbilityComponent comp, ToggleActionEvent args)
    {
    if (args.Handled)
        return;

    var xform = Transform(uid);
    var range = 1f;
    bool ventFound = false;

    if (xform.GridUid == null)
    {
        return;
    }

    var entitiesNearby = _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(uid, xform), range);

    foreach (var entity in entitiesNearby)
    {
        if (HasComp<GasVentPumpComponent>(entity) || HasComp<GasVentScrubberComponent>(entity))
        {
            if (TryComp<WeldableComponent>(entity, out var weldable) && weldable.IsWelded)
            {
                continue;
            }
            ventFound = true;
            break;
        }
    }

    if (!ventFound)
    {
        _popup.PopupEntity(Loc.GetString("Рядом нет подходящей вентиляции"), uid, uid);
        return;
    }

    args.Handled = true;

    var wasActive = comp.IsActive;
    comp.IsActive = !comp.IsActive;
    Dirty(uid, comp);

    if (comp.IsActive && !wasActive)
        EnterVent(uid, comp);
    else if (!comp.IsActive && wasActive)
        ExitVent(uid, comp);
    }

    private void OnShutdown(EntityUid uid, XenoVentAbilityComponent comp, ComponentShutdown args)
    {
    if (comp.IsActive)
        ExitVent(uid, comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<XenoVentAbilityComponent, TransformComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform, out var physics))
        {
            if (!comp.IsActive)
                continue;

            if (xform.GridUid != null)
            {
                comp.LastValidGridCoords = xform.Coordinates;
            }
            else
            {
                if (comp.LastValidGridCoords != null)
                {
                        _transform.SetCoordinates(uid, comp.LastValidGridCoords.Value);

                    _physics.SetLinearVelocity(uid, Vector2.Zero, body: physics);
                    _physics.SetAngularVelocity(uid, 0f, body: physics);
                }
                else
                {
                    _physics.SetLinearVelocity(uid, Vector2.Zero, body: physics);
                }
            }
        }
    }


    private void EnterVent(EntityUid uid, XenoVentAbilityComponent comp)
    {
        if (_tag.HasTag(uid, LargeCrawlerTag))
        {
            if (TryComp<FootstepModifierComponent>(uid, out var footstep) && comp.OriginalFootstepSound == null)
            {
                comp.OriginalFootstepSound = footstep.FootstepSoundCollection;
                footstep.FootstepSoundCollection = new SoundCollectionSpecifier("EventXenoVent");
                Dirty(uid, footstep);
            }
        }

        if (TryComp<FixturesComponent>(uid, out var fixtures) && TryComp<PhysicsComponent>(uid, out var physics))
        {
            if (comp.OriginalBodyType == null)
            {
                comp.OriginalBodyType = physics.BodyType;
            }
            if (comp.OriginalBodyStatus == null)
            {
                comp.OriginalBodyStatus = physics.BodyStatus;
            }

            foreach (var (fixtureId, fixture) in fixtures.Fixtures)
            {
                if (comp.OriginalCollisionLayer == null)
                {
                    comp.OriginalCollisionLayer = fixture.CollisionLayer;
                }
                if (comp.OriginalCollisionMask == null)
                {
                    comp.OriginalCollisionMask = fixture.CollisionMask;
                }

                _physics.SetCollisionMask(uid, fixtureId, fixture, (int) CollisionGroup.MapGrid, fixtures, physics);
                _physics.SetCollisionLayer(uid, fixtureId, fixture, (int) CollisionGroup.GhostImpassable, fixtures, physics);
            }

            _physics.SetBodyType(uid, BodyType.KinematicController, fixtures, physics, Transform(uid));
            _physics.SetBodyStatus(uid, physics, BodyStatus.InAir);
        }

        if (!HasComp<CanMoveInAirComponent>(uid))
        {
            EnsureComp<CanMoveInAirComponent>(uid);
            comp.AddedCanMoveInAir = true;
        }

        _actionBlocker.UpdateCanMove(uid);
        if (_tag.HasTag(uid, LargeCrawlerTag))
        {
            _audio.PlayGlobal(new SoundPathSpecifier("/Audio/_DeadSpace/TEMP_FOR_EVENT/Alien_Isolation/Xeno/vent_in.ogg"),
                Filter.Pvs(uid),
                false);
        }

        if (TryComp<EyeComponent>(uid, out var eye))
        {
            _eye.SetDrawFov(uid, false, eye);
        }
        Dirty(uid, comp);
    }

    private void ExitVent(EntityUid uid, XenoVentAbilityComponent comp)
    {
        if (TryComp<FootstepModifierComponent>(uid, out var footstep) && comp.OriginalFootstepSound != null)
        {
            footstep.FootstepSoundCollection = comp.OriginalFootstepSound;
            comp.OriginalFootstepSound = null;
            Dirty(uid, footstep);
        }

        if (TryComp<FixturesComponent>(uid, out var fixtures) && TryComp<PhysicsComponent>(uid, out var physics))
        {
            foreach (var (fixtureId, fixture) in fixtures.Fixtures)
            {
                if (comp.OriginalCollisionMask.HasValue)
                    _physics.SetCollisionMask(uid, fixtureId, fixture, comp.OriginalCollisionMask.Value, fixtures, physics);

                if (comp.OriginalCollisionLayer.HasValue)
                    _physics.SetCollisionLayer(uid, fixtureId, fixture, comp.OriginalCollisionLayer.Value, fixtures, physics);
            }

            if (comp.OriginalBodyType.HasValue)
                _physics.SetBodyType(uid, comp.OriginalBodyType.Value, fixtures, physics, Transform(uid));

            if (comp.OriginalBodyStatus.HasValue)
                _physics.SetBodyStatus(uid, physics, comp.OriginalBodyStatus.Value);

            comp.OriginalBodyType = null;
            comp.OriginalBodyStatus = null;
            comp.OriginalCollisionLayer = null;
            comp.OriginalCollisionMask = null;
        }

        if (comp.AddedCanMoveInAir && HasComp<CanMoveInAirComponent>(uid))
        {
            RemComp<CanMoveInAirComponent>(uid);
            comp.AddedCanMoveInAir = false;
        }

        _actionBlocker.UpdateCanMove(uid);

        if (_tag.HasTag(uid, LargeCrawlerTag))
        {
            _audio.PlayGlobal(new SoundPathSpecifier("/Audio/_DeadSpace/TEMP_FOR_EVENT/Alien_Isolation/Xeno/vent_out.ogg"),
                Filter.Pvs(uid),
                false);
        }

        if (TryComp<EyeComponent>(uid, out var eye))
        {
            _eye.SetDrawFov(uid, true, eye);
        }
        comp.LastValidGridCoords = null;
        Dirty(uid, comp);
    }
}
