using System.Numerics;
using Content.Shared.CombatMode;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Movement.Events;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics.Joints;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.Weapons.Misc;

public abstract class SharedHarpoonGunSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public const string HarpoonJoint = "harpoon";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HarpoonProjectileComponent, ProjectileEmbedEvent>(OnHarpoonCollide);
        SubscribeLocalEvent<HarpoonProjectileComponent, JointRemovedEvent>(OnHarpoonJointRemoved);
        SubscribeLocalEvent<CanWeightlessMoveEvent>(OnWeightlessMove);
        SubscribeAllEvent<RequestHarpoonReelMessage>(OnHarpoonReel);

        SubscribeLocalEvent<HarpoonGunComponent, GunShotEvent>(OnHarpoonShot);
        SubscribeLocalEvent<HarpoonGunComponent, ActivateInWorldEvent>(OnGunActivate);
        SubscribeLocalEvent<HarpoonGunComponent, HandDeselectedEvent>(OnHarpoonDeselected);
    }

    private void OnHarpoonJointRemoved(EntityUid uid, HarpoonProjectileComponent component, JointRemovedEvent args)
    {
        if (_netManager.IsServer)
            QueueDel(uid);
    }

    private void OnHarpoonShot(EntityUid uid, HarpoonGunComponent component, ref GunShotEvent args)
    {
        foreach (var (shotUid, _) in args.Ammo)
        {
            if (!HasComp<HarpoonProjectileComponent>(shotUid))
                continue;

            component.Projectile = shotUid.Value;
            Dirty(uid, component);
            var visuals = EnsureComp<JointVisualsComponent>(shotUid.Value);
            visuals.Sprite = component.RopeSprite;
            visuals.OffsetA = new Vector2(0f, 0.5f);
            visuals.Target = uid;
            Dirty(shotUid.Value, visuals);
        }

        TryComp<AppearanceComponent>(uid, out var appearance);
        _appearance.SetData(uid, SharedTetherGunSystem.TetherVisualsStatus.Key, false, appearance);
        Dirty(uid, component);
    }

    private void OnHarpoonDeselected(EntityUid uid, HarpoonGunComponent component, HandDeselectedEvent args)
    {
        SetReeling(uid, component, false, args.User);
    }

    private void OnHarpoonReel(RequestHarpoonReelMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player)
            return;

        if (!_hands.TryGetActiveItem(player, out var activeItem) ||
            !TryComp<HarpoonGunComponent>(activeItem, out var harpoon))
        {
            return;
        }

        if (msg.Reeling &&
            (!TryComp<CombatModeComponent>(player, out var combatMode) ||
             !combatMode.IsInCombatMode))
        {
            return;
        }

        SetReeling(activeItem.Value, harpoon, msg.Reeling, player);
    }

    private void OnWeightlessMove(ref CanWeightlessMoveEvent ev)
    {
        if (ev.CanMove || !TryComp<JointRelayTargetComponent>(ev.Uid, out var relayComp))
            return;

        foreach (var relay in relayComp.Relayed)
        {
            if (TryComp<JointComponent>(relay, out var jointRelay) && jointRelay.GetJoints.ContainsKey(HarpoonJoint))
            {
                ev.CanMove = true;
                return;
            }
        }
    }

    private void OnGunActivate(EntityUid uid, HarpoonGunComponent component, ActivateInWorldEvent args)
    {
        if (!Timing.IsFirstTimePredicted || args.Handled || !args.Complex || component.Projectile is not { } projectile)
            return;

        _audio.PlayPredicted(component.CycleSound, uid, args.User);
        _appearance.SetData(uid, SharedTetherGunSystem.TetherVisualsStatus.Key, true);

        if (_netManager.IsServer)
            QueueDel(projectile);

        component.Projectile = null;
        SetReeling(uid, component, false, args.User);
        _gun.ChangeBasicEntityAmmoCount(uid, 1);

        args.Handled = true;
    }

    private void SetReeling(EntityUid uid, HarpoonGunComponent component, bool value, EntityUid? user)
    {
        if (component.Reeling == value)
            return;

        if (value)
        {
            if (Timing.IsFirstTimePredicted)
                component.Stream = _audio.PlayPredicted(component.ReelSound, uid, user)?.Entity;
        }
        else
        {
            if (Timing.IsFirstTimePredicted)
                component.Stream = _audio.Stop(component.Stream);
        }

        component.Reeling = value;
        Dirty(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    
        var query = EntityQueryEnumerator<HarpoonGunComponent>();
    
        while (query.MoveNext(out var uid, out var harpoon))
        {
            if (!harpoon.Reeling || harpoon.Projectile == null)
            {
                if (Timing.IsFirstTimePredicted && harpoon.Stream != null)
                    harpoon.Stream = _audio.Stop(harpoon.Stream);
                continue;
            }
    
            if (!TryComp<HarpoonProjectileComponent>(harpoon.Projectile.Value, out var projectileComp))
                continue;
    
            if (projectileComp.HitTarget == null)
                continue; // ещё не попали
    
            var target = projectileComp.HitTarget.Value;
    
            if (!TryComp<PhysicsComponent>(target, out var physicsTarget))
            {
                SetReeling(uid, harpoon, false, null);
                continue;
            }
    
            var posShooter = Transform(uid).WorldPosition;
            var posTarget = Transform(target).WorldPosition;
    
            var direction = posShooter - posTarget;
            var distance = direction.Length();
    
            if (distance < 0.01f)
                continue;
    
            direction = Vector2.Normalize(direction);
    
            // Сила притяжения как в DistanceJoint: ReelRate * frameTime * масса цели
            // Можно умножить на множитель, если нужно сильнее
            var impulse = direction * harpoon.ReelRate * frameTime * physicsTarget.Mass;
    
            _physics.ApplyLinearImpulse(target, impulse);
            _physics.WakeBody(target);
        }
    }


    private void OnHarpoonCollide(EntityUid uid, HarpoonProjectileComponent component, ref ProjectileEmbedEvent args)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        // Сохраняем цель, в которую попал Harpoon
        component.HitTarget = args.Embedded;

        // Если нужен Joint для визуализации каната
        var jointComp = EnsureComp<JointComponent>(uid);
        var joint = _joints.CreateDistanceJoint(uid, args.Weapon, anchorA: new Vector2(0f, 0.5f), id: "harpoon");
        joint.MaxLength = joint.Length + 0.2f;
        joint.MinLength = 0.35f;
        joint.Stiffness = 1f;
        Dirty(uid, jointComp);
    }


    [Serializable, NetSerializable]
    protected sealed class RequestHarpoonReelMessage : EntityEventArgs
    {
        public bool Reeling;

        public RequestHarpoonReelMessage(bool reeling)
        {
            Reeling = reeling;
        }
    }
}
