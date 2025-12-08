using Content.Shared.Damage;
using Content.Shared.DeadSpace.IPC;
using Content.Shared.Emp;

namespace Content.Server.DeadSpace.IPC;

public sealed class DamageOnEMPSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageOnEMPComponent, EmpPulseEvent>(OnEMPPulse);
    }

    private void OnEMPPulse(EntityUid uid, DamageOnEMPComponent comp, ref EmpPulseEvent args)
    {
        args.Affected = true;

        var dmg = new DamageSpecifier();
        dmg.DamageDict.Add(comp.DamageType, comp.Damage);

        _damageable.TryChangeDamage(uid, dmg);
    }
}
