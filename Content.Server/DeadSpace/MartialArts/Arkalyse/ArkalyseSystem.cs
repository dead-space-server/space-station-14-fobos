// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Content.Server.DeadSpace.MartialArts.Arkalyse.Components;
using Content.Shared.DeadSpace.MartialArts.Arkalyse;
using Content.Shared.Mobs.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Robust.Shared.Audio;
using Content.Shared.Speech.Muting;
using Robust.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Content.Server.Damage.Systems;

namespace Content.Server.DeadSpace.MartialArts.Arkalyse;

public sealed class ArkalyseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseDamageEvent>(OnDamageAction);
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseStunEvent>(OnStunAction);
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseMuteEvent>(OnMuteAction);
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseRelaxEvent>(OnRelaxAction);
        SubscribeLocalEvent<ArkalyseComponent, MeleeHitEvent>(OnMeleeHitEvent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ArkalyseMutedComponent, MutedComponent>();
        while (query.MoveNext(out var uid, out var arkMuted, out _))
        {
            if (_timing.CurTime < arkMuted.MuteEndTime)
                continue;

            RemComp<MutedComponent>(uid);
            RemComp<ArkalyseMutedComponent>(uid);
        }
    }

    private void SelectCombo(Entity<ArkalyseComponent> ent, ArkalyseList combo)
    {
        ent.Comp.SelectedCombo = combo;

        _popup.PopupEntity(Loc.GetString("active-martial-ability"), ent, ent);
    }

    private void OnDamageAction(Entity<ArkalyseComponent> ent, ref ArkalyseDamageEvent args)
    {
        if (args.Handled)
            return;

        SelectCombo(ent, ArkalyseList.DamageAttack);

        args.Handled = true;
    }
    private void OnStunAction(Entity<ArkalyseComponent> ent, ref ArkalyseStunEvent args)
    {
        if (args.Handled)
            return;

        SelectCombo(ent, ArkalyseList.StunAttack);

        args.Handled = true;
    }

    private void OnMuteAction(Entity<ArkalyseComponent> ent, ref ArkalyseMuteEvent args)
    {
        if (args.Handled)
            return;

        SelectCombo(ent, ArkalyseList.MuteAttack);

        args.Handled = true;
    }

    private void OnRelaxAction(Entity<ArkalyseComponent> ent, ref ArkalyseRelaxEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.SelectedCombo = ArkalyseList.RelaxHand;
        _popup.PopupEntity(Loc.GetString("relax-martial-ability"), ent, ent);

        args.Handled = true;
    }
    private void OnMeleeHitEvent(Entity<ArkalyseComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        foreach (var hitEntity in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(hitEntity))
                continue;

            DoHitArkalyse(ent, hitEntity);
        }
    }

    private void DoHitArkalyse(Entity<ArkalyseComponent> ent, EntityUid hitEntity)
    {
        if (ent.Comp.SelectedCombo is not { } combo)
            return;

        switch (combo)
        {
            case ArkalyseList.DamageAttack:
                DamageHit(hitEntity, ent.Comp.Params.DamageTypeForDamageAtack, ent.Comp.Params.HitDamageForDamageAtack, ent.Comp.Params.IgnoreResist, out _);
                SpawnAttachedTo(ent.Comp.Params.EffectPunchForDamageAtack, Transform(hitEntity).Coordinates);
                _audio.PlayPvs(ent.Comp.Params.HitSoundForDamageAtack, hitEntity, AudioParams.Default.WithVolume(3.0f));
                break;

            case ArkalyseList.StunAttack:
                _audio.PlayPvs(ent.Comp.Params.HitSoundForStunAtack, hitEntity, AudioParams.Default.WithVolume(0.5f));
                _stun.TryUpdateParalyzeDuration(hitEntity, TimeSpan.FromSeconds(ent.Comp.Params.ParalyzeTimeStunAtack));
                SpawnAttachedTo(ent.Comp.Params.EffectPunchForStunAtack, Transform(hitEntity).Coordinates);
                break;

            case ArkalyseList.MuteAttack:
                var muted = EnsureComp<ArkalyseMutedComponent>(hitEntity);
                EnsureComp<MutedComponent>(hitEntity);
                muted.MuteEndTime = _timing.CurTime + ent.Comp.Params.ParalyzeTimeMuteAtack;
                DamageHit(hitEntity, ent.Comp.Params.DamageTypeForMuteAtack, ent.Comp.Params.HitDamageForMuteAtack, ent.Comp.Params.IgnoreResist, out _);
                _stamina.TakeStaminaDamage(hitEntity, ent.Comp.Params.StaminaDamageMuteAtack);
                break;

            case ArkalyseList.RelaxHand:
                break;
        }
        ent.Comp.SelectedCombo = null;
    }

    private void DamageHit(EntityUid target,
    string damageType,
    int damageAmount,
    bool ignoreResist,
    out DamageSpecifier damage)
    {
        damage = new DamageSpecifier();
        damage.DamageDict.Add(damageType, damageAmount);

        _damageable.TryChangeDamage(target, damage, ignoreResist);
    }
}