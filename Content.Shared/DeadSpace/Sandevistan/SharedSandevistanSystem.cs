// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Shared.DeadSpace.Sandevistan;

public sealed class SharedSandevistanSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActiveSandevistanComponent, ComponentStartup>(OnActiveStartup);
        SubscribeLocalEvent<ActiveSandevistanComponent, ComponentShutdown>(OnActiveShutdown);
        SubscribeLocalEvent<ActiveSandevistanComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<ActiveSandevistanComponent, GetMeleeAttackRateEvent>(OnGetMeleeAttackRate);
        SubscribeLocalEvent<SandevistanRecoveryComponent, ComponentStartup>(OnRecoveryStartup);
        SubscribeLocalEvent<SandevistanRecoveryComponent, ComponentShutdown>(OnRecoveryShutdown);
        SubscribeLocalEvent<SandevistanRecoveryComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshRecoveryMovementSpeed);
        SubscribeLocalEvent<SandevistanSpeedFadeoutComponent, ComponentStartup>(OnSpeedFadeoutStartup);
        SubscribeLocalEvent<SandevistanSpeedFadeoutComponent, ComponentShutdown>(OnSpeedFadeoutShutdown);
        SubscribeLocalEvent<SandevistanSpeedFadeoutComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeedFadeoutMovementSpeed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var recoveryQuery = EntityQueryEnumerator<SandevistanRecoveryComponent>();
        while (recoveryQuery.MoveNext(out var uid, out _))
        {
            if (Paused(uid))
                continue;

            _movement.RefreshMovementSpeedModifiers(uid);
        }

        var fadeoutQuery = EntityQueryEnumerator<SandevistanSpeedFadeoutComponent>();
        while (fadeoutQuery.MoveNext(out var uid, out _))
        {
            if (Paused(uid))
                continue;

            _movement.RefreshMovementSpeedModifiers(uid);
        }
    }

    private void OnActiveStartup(Entity<ActiveSandevistanComponent> ent, ref ComponentStartup args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnActiveShutdown(Entity<ActiveSandevistanComponent> ent, ref ComponentShutdown args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnRefreshMovementSpeed(Entity<ActiveSandevistanComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.MovementSpeedModifier);
    }

    private void OnGetMeleeAttackRate(Entity<ActiveSandevistanComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        if (args.User != ent.Owner || HasComp<GunComponent>(args.Weapon))
            return;

        args.Multipliers *= ent.Comp.AttackRateModifier;
    }

    private void OnRecoveryStartup(Entity<SandevistanRecoveryComponent> ent, ref ComponentStartup args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnRecoveryShutdown(Entity<SandevistanRecoveryComponent> ent, ref ComponentShutdown args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnRefreshRecoveryMovementSpeed(Entity<SandevistanRecoveryComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var remaining = MathF.Max(0f, (float) (ent.Comp.EndTime - _timing.CurTime).TotalSeconds);
        var progress = SmoothStep(1f - Math.Clamp(remaining / MathF.Max(ent.Comp.Duration, 0.1f), 0f, 1f));
        var modifier = MathHelper.Lerp(ent.Comp.MovementSpeedModifier, 1f, progress);

        args.ModifySpeed(modifier);
    }

    private void OnSpeedFadeoutStartup(Entity<SandevistanSpeedFadeoutComponent> ent, ref ComponentStartup args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnSpeedFadeoutShutdown(Entity<SandevistanSpeedFadeoutComponent> ent, ref ComponentShutdown args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnRefreshSpeedFadeoutMovementSpeed(Entity<SandevistanSpeedFadeoutComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var remaining = MathF.Max(0f, (float) (ent.Comp.EndTime - _timing.CurTime).TotalSeconds);
        var progress = SmoothStep(Math.Clamp(remaining / MathF.Max(ent.Comp.Duration, 0.1f), 0f, 1f));
        var modifier = MathHelper.Lerp(ent.Comp.EndModifier, ent.Comp.StartModifier, progress);

        args.ModifySpeed(modifier);
    }

    private static float SmoothStep(float progress)
    {
        return progress * progress * (3f - 2f * progress);
    }
}
