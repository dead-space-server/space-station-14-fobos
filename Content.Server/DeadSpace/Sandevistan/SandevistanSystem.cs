// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DeadSpace.Sandevistan;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Jittering;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Sandevistan;

public sealed class SandevistanSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanImplantComponent, ActivateSandevistanImplantEvent>(OnActivated);
        SubscribeLocalEvent<SandevistanImplantComponent, ImplantRemovedEvent>(OnImplantRemoved);
        SubscribeLocalEvent<SandevistanImplantComponent, ComponentShutdown>(OnImplantShutdown);
        SubscribeLocalEvent<MeleeWeaponComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ActiveSandevistanComponent>();
        while (query.MoveNext(out var uid, out var active))
        {
            if (Paused(uid))
                continue;

            if (ShouldStop(uid, active) || curTime >= active.EndTime)
            {
                StopSandevistan(uid);
                continue;
            }

            ApplyJitter(uid, active, frameTime);

            if (curTime >= active.NextOverloadTime)
                ApplyOverloadTicks(uid, active, curTime);
        }
    }

    private void OnActivated(EntityUid uid, SandevistanImplantComponent component, ActivateSandevistanImplantEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<SubdermalImplantComponent>(uid, out var subdermal) ||
            subdermal.ImplantedEntity is not { } target ||
            Deleted(target))
        {
            return;
        }

        if (HasComp<ActiveSandevistanComponent>(target))
        {
            args.Handled = true;
            return;
        }

        var curTime = _timing.CurTime;
        var active = EnsureComp<ActiveSandevistanComponent>(target);
        active.SourceImplant = uid;
        active.EndTime = curTime + TimeSpan.FromSeconds(component.Duration);
        active.SoftcapTime = curTime + TimeSpan.FromSeconds(component.SoftcapTime);
        active.NextOverloadTime = active.SoftcapTime + GetInterval(component.OverloadInterval);
        active.MovementSpeedModifier = component.MovementSpeedModifier;
        active.AttackRateModifier = component.AttackRateModifier;
        active.OverloadInterval = component.OverloadInterval;
        active.OverloadStaminaDamage = component.OverloadStaminaDamage;
        active.OverloadDamage = new(component.OverloadDamage);
        active.InitialJitterProgress = Math.Clamp(component.InitialJitterProgress, 0f, 1f);
        active.JitterCurrentProgress = 0f;
        active.JitterTargetProgress = active.InitialJitterProgress;
        active.JitterHits = 0;
        active.MaxJitterHits = component.MaxJitterHits;
        active.MaxJitterAmplitude = component.MaxJitterAmplitude;
        active.MaxJitterFrequency = component.MaxJitterFrequency;
        active.JitterLerpRate = component.JitterLerpRate;
        active.JitterRefreshTime = component.JitterRefreshTime;
        active.AfterimageInterval = component.AfterimageInterval;
        active.AfterimageMinDistance = component.AfterimageMinDistance;
        active.AfterimageLifetime = component.AfterimageLifetime;
        active.AfterimageColor = component.AfterimageColor;
        active.AfterimageFallbackEffect = component.AfterimageFallbackEffect;

        Dirty(target, active);
        _movement.RefreshMovementSpeedModifiers(target);
        ApplyJitter(target, active, 0f);

        if (component.Popup is { } popup)
            _popup.PopupEntity(Loc.GetString(popup), target, target);

        args.Handled = true;
    }

    private void OnImplantRemoved(Entity<SandevistanImplantComponent> ent, ref ImplantRemovedEvent args)
    {
        if (!TryComp<ActiveSandevistanComponent>(args.Implanted, out var active) ||
            active.SourceImplant != ent.Owner)
        {
            return;
        }

        StopSandevistan(args.Implanted);
    }

    private void OnImplantShutdown(Entity<SandevistanImplantComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SubdermalImplantComponent>(ent.Owner, out var subdermal) ||
            subdermal.ImplantedEntity is not { } target ||
            !TryComp<ActiveSandevistanComponent>(target, out var active) ||
            active.SourceImplant != ent.Owner)
        {
            return;
        }

        StopSandevistan(target);
    }

    private void OnMeleeHit(EntityUid uid, MeleeWeaponComponent component, MeleeHitEvent args)
    {
        if (!args.IsHit ||
            !args.HitEntities.Any() ||
            HasComp<GunComponent>(args.Weapon) ||
            !TryComp<ActiveSandevistanComponent>(args.User, out var active))
        {
            return;
        }

        active.JitterHits = Math.Min(active.JitterHits + 1, Math.Max(active.MaxJitterHits, 1));

        var hitProgress = active.MaxJitterHits <= 0
            ? 1f
            : active.JitterHits / (float) active.MaxJitterHits;

        active.JitterTargetProgress = Math.Clamp(
            MathHelper.Lerp(active.InitialJitterProgress, 1f, hitProgress),
            active.InitialJitterProgress,
            1f);
    }

    private bool ShouldStop(EntityUid uid, ActiveSandevistanComponent active)
    {
        if (active.SourceImplant is not { } implant ||
            Deleted(implant) ||
            !TryComp<SubdermalImplantComponent>(implant, out var subdermal) ||
            subdermal.ImplantedEntity != uid)
        {
            return true;
        }

        return false;
    }

    private void StopSandevistan(EntityUid uid)
    {
        RemCompDeferred<ActiveSandevistanComponent>(uid);
    }

    private void ApplyJitter(EntityUid uid, ActiveSandevistanComponent active, float frameTime)
    {
        var lerp = Math.Clamp(frameTime * active.JitterLerpRate, 0f, 1f);
        active.JitterCurrentProgress = MathHelper.Lerp(active.JitterCurrentProgress, active.JitterTargetProgress, lerp);

        var amplitude = active.MaxJitterAmplitude * active.JitterCurrentProgress;
        var frequency = active.MaxJitterFrequency * active.JitterCurrentProgress;

        _jittering.DoJitter(
            uid,
            TimeSpan.FromSeconds(MathF.Max(active.JitterRefreshTime, 0.1f)),
            true,
            amplitude,
            frequency,
            true);

        if (TryComp<JitteringComponent>(uid, out var jittering))
            Dirty(uid, jittering);
    }

    private void ApplyOverloadTicks(EntityUid uid, ActiveSandevistanComponent active, TimeSpan curTime)
    {
        var interval = GetInterval(active.OverloadInterval);
        while (curTime >= active.NextOverloadTime)
        {
            if (ApplyOverload(uid, active))
            {
                StopSandevistan(uid);
                return;
            }

            active.NextOverloadTime += interval;
        }
    }

    private bool ApplyOverload(EntityUid uid, ActiveSandevistanComponent active)
    {
        if (TryComp<DamageableComponent>(uid, out var damageable))
        {
            _damageable.TryChangeDamage(
                (uid, damageable),
                active.OverloadDamage,
                ignoreResistances: true,
                interruptsDoAfters: false,
                origin: uid,
                ignoreGlobalModifiers: true);
        }

        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return false;

        _stamina.TakeStaminaDamage(
            uid,
            active.OverloadStaminaDamage,
            stamina,
            source: uid,
            visual: true,
            ignoreResist: true);

        return stamina.Critical;
    }

    private static TimeSpan GetInterval(float seconds)
    {
        return TimeSpan.FromSeconds(MathF.Max(seconds, 0.1f));
    }
}
