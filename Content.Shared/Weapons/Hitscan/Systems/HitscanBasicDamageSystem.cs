using Content.Shared.Damage.Systems;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Whitelist;

namespace Content.Shared.Weapons.Hitscan.Systems;

public sealed class HitscanBasicDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitscanBasicDamageComponent, HitscanRaycastFiredEvent>(OnHitscanHit);
    }

    private void OnHitscanHit(Entity<HitscanBasicDamageComponent> ent, ref HitscanRaycastFiredEvent args)
    {
        if (args.Data.HitEntity == null)
            return;

        if (!_whitelist.CheckBoth(args.Data.HitEntity, ent.Comp.Blacklist, ent.Comp.Whitelist))
            return;

        var dmg = ent.Comp.Damage * _damage.UniversalHitscanDamageModifier;

        if (!_damage.TryChangeDamage(args.Data.HitEntity.Value, dmg, out var damageDealt, ent.Comp.IgnoreResistances, origin: args.Data.Shooter ?? args.Data.Gun)) // DS14
            return;

        var damageEvent = new HitscanDamageDealtEvent
        {
            Target = args.Data.HitEntity.Value,
            DamageDealt = damageDealt,
            Data = args.Data,
        };

        RaiseLocalEvent(ent, ref damageEvent);
    }
}
