using Content.Server.DeadSpace.MartialArts.Arkalyse.Components;
using Content.Shared.DeadSpace.MartialArts.Arkalyse;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio;
using Content.Shared.Speech.Muting;
using Robust.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage;
using Content.Shared.Popups;
using System.Linq;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Content.Server.Damage.Systems;

namespace Content.Server.DeadSpace.MartialArts.Arkalyse;

public partial class ServerArkalyseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseDamageEvent>(OnDamageAction);
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseStunEvent>(OnStunAction);
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseMuteEvent>(OnMuteAction);
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseRelaxEvent>(OnRelaxAction);
        SubscribeLocalEvent<ArkalyseComponent, MeleeHitEvent>(OnMeleeHitEvent);
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

        SelectCombo(ent, ArkalyseList.DamageAtack);

        args.Handled = true;
    }
    private void OnStunAction(Entity<ArkalyseComponent> ent, ref ArkalyseStunEvent args)
    {
        if (args.Handled)
            return;

        SelectCombo(ent, ArkalyseList.StunAtack);

        args.Handled = true;
    }

    private void OnMuteAction(Entity<ArkalyseComponent> ent, ref ArkalyseMuteEvent args)
    {
        if (args.Handled)
            return;

        SelectCombo(ent, ArkalyseList.MuteAtack);

        args.Handled = true;
    }

    private void OnRelaxAction(Entity<ArkalyseComponent> ent, ref ArkalyseRelaxEvent args)
    {
        if (args.Handled)
            return;

        SelectCombo(ent, ArkalyseList.RelaxHand);
        _popup.PopupEntity(Loc.GetString("relax-martial-ability"), ent, ent);

        args.Handled = true;
    }
    private void OnMeleeHitEvent(Entity<ArkalyseComponent> ent, ref MeleeHitEvent args)
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

    private void DoHitArkalyse(Entity<ArkalyseComponent> ent, EntityUid hitEntity)
    {
        if (ent.Comp.SelectedCombo is not { } combo)
            return;

        switch (combo)
        {
            case ArkalyseList.DamageAtack:
                DamageHit(hitEntity, ent.Comp.Params.DamageTypeForDamageAtack, ent.Comp.Params.HitDamageForDamageAtack, ent.Comp.Params.IgnoreResist, out _);
                SpawnAttachedTo(ent.Comp.Params.EffectPunchForDamageAtack, Transform(hitEntity).Coordinates);
                _audio.PlayPvs(ent.Comp.Params.HitSoundForDamageAtack, ent, AudioParams.Default.WithVolume(3.0f));
                break;

            case ArkalyseList.StunAtack:
                _audio.PlayPvs(ent.Comp.Params.HitSoundForStunAtack, ent, AudioParams.Default.WithVolume(0.5f));
                _stun.TryUpdateParalyzeDuration(hitEntity, TimeSpan.FromSeconds(ent.Comp.Params.ParalyzeTimeStunAtack));
                SpawnAttachedTo(ent.Comp.Params.EffectPunchForStunAtack, Transform(hitEntity).Coordinates);
                break;

            case ArkalyseList.MuteAtack:
                EnsureComp<MutedComponent>(hitEntity);
                Timer.Spawn(TimeSpan.FromSeconds(ent.Comp.Params.ParalyzeTimeMuteAtack), () => { if (Exists(hitEntity)) RemComp<MutedComponent>(hitEntity); });
                DamageHit(hitEntity, ent.Comp.Params.DamageTypeForMuteAtack, ent.Comp.Params.HitDamageForMuteAtack, ent.Comp.Params.IgnoreResist, out _);
                _stamina.TakeStaminaDamage(hitEntity, ent.Comp.Params.StaminaDamageMuteAtack);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(combo), combo, null);
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
