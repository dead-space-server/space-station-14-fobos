using Content.Server.DeadSpace.MartialArts;
using Content.Server.DeadSpace.MartialArts.Arkalyse.Component;
using Content.Server.DeadSpace.MartialArts.SmokingCarp.Component;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Weapons.Reflect;
using Content.Shared.DeadSpace.MartialArts.SmokingCarp;
using Content.Shared.DeadSpace.MartialArts.Arkalyse;

namespace Content.Server.DeadSpace.MartialArts.SmokingCarp;

public partial class ServerMartialArtsSystem
{
    private void InitializeSmokingCarp()
    {
        SubscribeLocalEvent<SmokingCarpComponent, SmokingCarpPowerPunchEvent>(OnPowerPunchAction);
        SubscribeLocalEvent<SmokingCarpComponent, SmokingCarpSmokePunchEvent>(OnSmokePunchAction);
        SubscribeLocalEvent<SmokingCarpComponent, MeleeHitEvent>(OnMeleeHitEvent);
        SubscribeLocalEvent<SmokingCarpComponent, ReflectCarpEvent>(SmokingCarpReflect);
        SubscribeLocalEvent<SmokingCarpTripPunchComponent, SmokingCarpTripPunchEvent>(SmokingCarpTripPunch);
    }

    private void SelectCombo(Entity<SmokingCarpComponent> ent, ref bool handled, SmokingCarpList combo)
    {
        if (handled)
            return;

        ent.Comp.SelectedCombo = combo;
        handled = true;

        _popup.PopupEntity(Loc.GetString("active-martial-ability"), ent, ent);
    }

    private void OnPowerPunchAction(Entity<SmokingCarpComponent> ent, ref SmokingCarpPowerPunchEvent args)
    {
        SelectCombo(ent, ref args.Handled, SmokingCarpList.PowerPunch);
    }

    private void OnSmokePunchAction(Entity<SmokingCarpComponent> ent, ref SmokingCarpSmokePunchEvent args)
    {
        SelectCombo(ent, ref args.Handled, SmokingCarpList.SmokePunch);
    }

    private void OnMeleeHitEvent(Entity<SmokingCarpComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.HitEntities.Any())
            return;

        foreach (var hitEntity in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(hitEntity))
                continue;

            DoHitArkalyse(ent, hitEntity);
        }
    }

    private void DoHitCarp(Entity<SmokingCarpComponent> ent, EntityUid hitEntity)
    {
    if (ent.Comp.SelectedCombo is not { } combo)
            return;

        switch (combo)
        {
            case SmokingCarpList.PowerPunch:
                DamageHit(hitEntity, ent.Comp.Params.DamageTypeForPowerPunch, ent.Comp.Params.HitDamageForPowerPunch, ent.Comp.Params.IgnoreResist, out _);
                SpawnAttachedTo(ent.Comp.Params.EffectPowerPunch, Transform(hitEntity).Coordinates);
                _audio.PlayPvs(ent.Comp.Params.HitSoundForPowerPunch, ent, AudioParams.Default.WithVolume(3.0f));

                var saying =
                Enumerable.ElementAt<LocId>(ent.Comp.Params.PackMessageOnHit, (int)_random.Next(ent.Comp.Params.PackMessageOnHit.Count));
                var ev = new SmokingCarpSaying(saying);
                RaiseLocalEvent(ent, ev);

                if (TryComp<PhysicsComponent>(hitEntity, out var physicsComponent))
                {
                    var userTransform = Transform(ent);
                    var targetTransform = Transform(hitEntity);
                    var pushDirection = _transform.GetWorldPosition(targetTransform) - _transform.GetWorldPosition(userTransform);

                    if (!pushDirection.Equals(Vector2.Zero))
                    {
                        var distance = pushDirection.Length();

                        if (distance <= ent.Comp.Params.MaxPushDistance)
                        {
                            pushDirection = pushDirection.Normalized();
                            var pushStrength = ent.Comp.Params.PushStrength;

                            pushStrength *= 10f - distance / ent.Comp.Params.MaxPushDistance;

                            var impulse = pushDirection * pushStrength;
                            _physics.ApplyLinearImpulse(hitEntity, impulse, body: physicsComponent);
                        }
                    }
                }
                break;

            case SmokingCarpList.SmokePunch:
                DamageHit(hitEntity, ent.Comp.Params.DamageTypeForSmokePunch, ent.Comp.Params.HitDamageForSmokePunch, ent.Comp.Params.IgnoreResist, out _);
                _stamina.TakeStaminaDamage(hitEntity, ent.Comp.Params.StaminaDamageSmokePunch);
                _audio.PlayPvs(ent.Comp.Params.HitSoundForSmokePunch, ent, AudioParams.Default.WithVolume(3.0f));
                SpawnAttachedTo(ent.Comp.Params.EffectSmokePunch, Transform(hitEntity).Coordinates);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(combo), combo, null);
        }
        ent.Comp.SelectedCombo = null;
        Dirty(ent);
    }

    private void SmokingCarpReflect(Entity<SmokingCarpComponent> ent, ref ReflectCarpEvent args)
    {
        if (HasComp<ReflectComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("unreflect-smoking-carp"), ent, ent);
            RemComp<ReflectComponent>(ent);
            RemComp<PacifiedComponent>(ent);
            return;
        }

        if (args.Handled)
            return;

        args.Handled = true;

        AddComp<PacifiedComponent>(ent);
        var reflectComponent = EnsureComp<ReflectComponent>(ent);
        _popup.PopupEntity(Loc.GetString("reflect-smoking-carp"), ent, ent);
        reflectComponent.ReflectProb = 1.0f;
        reflectComponent.Spread = 360f;
    }

    private void SmokingCarpTripPunch(Entity<SmokingCarpTripPunchComponent> ent, ref SmokingCarpTripPunchEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var xform = Transform(args.Performer);

        _receivers.Clear();

        foreach (var target in _entityLookup.GetEntitiesInRange(xform.Coordinates, ent.Comp.Range))
        {
            if (target == args.Performer)
                continue;

            if (HasComp<SmokingCarpComponent>(target))
                continue;

            if (!HasComp<MobStateComponent>(target))
                continue;

            _receivers.Add(target);
        }
            _audio.PlayPvs(ent.Comp.TripSound, args.Performer);

        foreach (var receiver in _receivers)
        {
            if (_mobState.IsDead(receiver))
                continue;

            _stun.TryUpdateParalyzeDuration(receiver, TimeSpan.FromSeconds(ent.Comp.ParalyzeTime), true);
        }

        if (ent.Comp.SelfEffect is not null)
            SpawnAttachedTo(ent.Comp.SelfEffect, Transform(args.Performer).Coordinates);
    }
}
