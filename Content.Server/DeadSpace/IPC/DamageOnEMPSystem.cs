// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.IPC.Components;
using Content.Shared.Damage;
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

        var scaledDamage = comp.Damage * args.EnergyConsumption;
        var dmg = new DamageSpecifier();
        dmg.DamageDict.Add(comp.DamageType, scaledDamage);

        _damageable.TryChangeDamage(uid, dmg);
    }
}
