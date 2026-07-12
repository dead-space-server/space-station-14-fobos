using Content.Shared._CM14.Weapons.Ranged;
using Content.Shared.FixedPoint;
using Content.Shared.Projectiles;
using Robust.Shared.Timing;
namespace Content.Shared._CM14.Weapons.Ranged;
public sealed class CMGunSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    private EntityQuery<ProjectileComponent> _projectileQuery;
    public override void Initialize()
    {
        base.Initialize();
        _projectileQuery = GetEntityQuery<ProjectileComponent>();
        SubscribeLocalEvent<GunDamageModifierComponent, GetGunDamageModifierEvent>(OnGunDamageModifier);
        SubscribeLocalEvent<GunDamageModifierComponent, MapInitEvent>(OnMapInit);
    }
    private void OnMapInit(Entity<GunDamageModifierComponent> ent, ref MapInitEvent args)
    {
        RefreshGunDamageMultiplier(ent.AsNullable());
    }
    private void OnGunDamageModifier(Entity<GunDamageModifierComponent> ent, ref GetGunDamageModifierEvent args)
    {
        args.Multiplier += ent.Comp.Multiplier;
    }
    public void RefreshGunDamageMultiplier(Entity<GunDamageModifierComponent?> gun)
    {
        gun.Comp = EnsureComp<GunDamageModifierComponent>(gun);
        var ev = new GetGunDamageModifierEvent(gun.Comp.Multiplier);
        RaiseLocalEvent(gun, ref ev);
        gun.Comp.ModifiedMultiplier = ev.Multiplier;
    }
    public void RefreshGunDamageMultiplier(EntityUid gun)
    {
        RefreshGunDamageMultiplier((gun, null));
    }
    public override void Update(float frameTime)
    {
        var time = _timing.CurTime;
    }
}
