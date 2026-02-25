using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Server.Damage.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.GameObjects;

namespace Content.Server.Weapons.Melee.Vampire;

public sealed class VampireHealSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        
        SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(EntityUid target, DamageableComponent comp, ref DamageChangedEvent args)
    {
        
        if (args.DamageDelta == null)
            return;

        
        if (args.Origin == null)
            return;

        var attacker = args.Origin.Value;

        
        var totalDamage = args.DamageDelta.GetTotal();
        if (totalDamage <= FixedPoint2.Zero)
            return;

        
        if (attacker == target)
            return;

        
        if (!TryComp<MobStateComponent>(target, out var mobState))
            return;

        
        if (mobState.CurrentState == MobState.Dead)
            return;

        
        if (!_hands.TryGetActiveItem(attacker, out var weapon))
            return;

        
        if (!TryComp<VampireHealComponent>(weapon.Value, out var vampire))
            return;

        

        var healAmount = totalDamage * vampire.HealMultiplier;

        if (healAmount <= FixedPoint2.Zero)
            return;

        

        if (!TryComp<DamageableComponent>(attacker, out var attackerDamage))
            return;

        var healSpecifier = new DamageSpecifier();

        foreach (var (type, amount) in attackerDamage.Damage.DamageDict)
        {
            if (amount > FixedPoint2.Zero)
            {
                healSpecifier.DamageDict[type] = -healAmount;
            }
        }

        if (healSpecifier.DamageDict.Count == 0)
            return;

        _damageable.TryChangeDamage(attacker, healSpecifier, origin: weapon.Value);
    }
}