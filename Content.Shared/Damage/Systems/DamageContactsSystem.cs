using Content.Shared.Damage.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Content.Shared.Inventory; //DS14
using Content.Shared.DeadSpace.Damage.Components; //DS14

namespace Content.Shared.Damage.Systems;

public sealed class DamageContactsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!; //DS14

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageContactsComponent, StartCollideEvent>(OnEntityEnter);
        SubscribeLocalEvent<DamageContactsComponent, EndCollideEvent>(OnEntityExit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DamagedByContactComponent>();

        while (query.MoveNext(out var ent, out var damaged))
        {
            //DS14 Start
            if (_inventory.TryGetSlotEntity(ent, "outerClothing", out var suit))
            {
                if (HasComp<IgnoreContactDamageComponent>(suit))
                {
                    RemComp<DamagedByContactComponent>(ent);
                    continue;
                }
            }
            //DS14 End

            if (_timing.CurTime < damaged.NextSecond)
                continue;
            damaged.NextSecond = _timing.CurTime + TimeSpan.FromSeconds(1);

            if (damaged.Damage != null)
                _damageable.TryChangeDamage(ent, damaged.Damage, interruptsDoAfters: false);
        }
        //DS14 Start
        var contactQuery = EntityQueryEnumerator<DamageContactsComponent>();

        while (contactQuery.MoveNext(out var source, out var contact))
        {
            if (!TryComp<PhysicsComponent>(source, out var body))
                continue;

            foreach (var ent in _physics.GetContactingEntities(source, body))
            {
                if (HasComp<DamagedByContactComponent>(ent))
                    continue;

                if (_whitelistSystem.IsWhitelistPass(contact.IgnoreWhitelist, ent))
                    continue;

                if (_inventory.TryGetSlotEntity(ent, "outerClothing", out var suit))
                {
                    if (HasComp<IgnoreContactDamageComponent>(suit))
                        continue;
                }

                var damagedByContact = EnsureComp<DamagedByContactComponent>(ent);
                damagedByContact.Damage = contact.Damage;
            }
        }
        //DS14 End
    }

    private void OnEntityExit(EntityUid uid, DamageContactsComponent component, ref EndCollideEvent args)
    {
        var otherUid = args.OtherEntity;

        if (!TryComp<PhysicsComponent>(otherUid, out var body))
            return;

        var damageQuery = GetEntityQuery<DamageContactsComponent>();
        foreach (var ent in _physics.GetContactingEntities(otherUid, body))
        {
            if (ent == uid)
                continue;

            if (damageQuery.HasComponent(ent))
                return;
        }

        RemComp<DamagedByContactComponent>(otherUid);
    }

    private void OnEntityEnter(EntityUid uid, DamageContactsComponent component, ref StartCollideEvent args)
    {
        var otherUid = args.OtherEntity;

        //DS14 Start
        if (_inventory.TryGetSlotEntity(otherUid, "outerClothing", out var suit))
        {
            if (HasComp<IgnoreContactDamageComponent>(suit))
                return;
        }
        //DS14 End

        if (HasComp<DamagedByContactComponent>(otherUid))
            return;

        if (_whitelistSystem.IsWhitelistPass(component.IgnoreWhitelist, otherUid))
            return;

        var damagedByContact = EnsureComp<DamagedByContactComponent>(otherUid);
        damagedByContact.Damage = component.Damage;
    }
}
