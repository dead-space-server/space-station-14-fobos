using Content.Server.DeadSpace.MartialArts;
using Content.Server.DeadSpace.MartialArts.Arkalyse.Component;
using Content.Shared.Interaction.Events;
using Content.Shared.DeadSpace.MartialArts.Arkalyse;
using Content.Shared.Weapons.Melee;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio;
using Content.Shared.Speech.Muting;
using Robust.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server.DeadSpace.MartialArts.Arkalyse;

public partial class ServerMartialArtsSystem
{
    private void InitializeArkalyse()
    {
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseDamageEvent>(OnDamageAction);
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseStunEvent>(OnStunAction);
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseMuteEvent>(OnMuteAction);
        SubscribeLocalEvent<ArkalyseComponent, ArkalyseRelaxEvent>(OnRelaxAction);
        SubscribeLocalEvent<ArkalyseComponent, MeleeHitEvent>(OnMeleeHitEvent);
    }

    private void SelectCombo(Entity<ArkalyseComponent> ent, ref bool handled, ArkalyseList combo)
    {
        if (handled)
            return;

        ent.Comp.SelectedCombo = combo;
        handled = true;

        _popup.PopupEntity(Loc.GetString("active-martial-ability"), ent, ent);
    }

    private void OnDamageAction(Entity<ArkalyseComponent> ent, ref ArkalyseDamageEvent args)
    {
        SelectCombo(ent, ref args.Handled, ArkalyseList.DamageAtack);
    }
    private void OnStunAction(Entity<ArkalyseComponent> ent, ref ArkalyseStunEvent args)
    {
        SelectCombo(ent, ref args.Handled, ArkalyseList.StunAtack);
    }

    private void OnMuteAction(Entity<ArkalyseComponent> ent, ref ArkalyseMuteEvent args)
    {
        SelectCombo(ent, ref args.Handled, ArkalyseList.MuteAtack);
    }

    private void OnRelaxAction(Entity<ArkalyseComponent> ent, ref ArkalyseRelaxEvent args)
    {
        SelectCombo(ent, ref args.Handled, ArkalyseList.RelaxHand);
        _popup.PopupEntity(Loc.GetString("relax-martial-ability"), ent, ent);
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
                _stun.TryUpdateParalyzeDuration(hitEntity, TimeSpan.FromSeconds(ent.Comp.Params.ParalyzeTimeStunAtack), true);
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
        Dirty(ent);
    }
}
