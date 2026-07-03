// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.DeadSpace.Sandevistan;

public sealed class SharedSandevistanSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActiveSandevistanComponent, ComponentStartup>(OnActiveStartup);
        SubscribeLocalEvent<ActiveSandevistanComponent, ComponentShutdown>(OnActiveShutdown);
        SubscribeLocalEvent<ActiveSandevistanComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<ActiveSandevistanComponent, GetMeleeAttackRateEvent>(OnGetMeleeAttackRate);
    }

    private void OnActiveStartup(Entity<ActiveSandevistanComponent> ent, ref ComponentStartup args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnActiveShutdown(Entity<ActiveSandevistanComponent> ent, ref ComponentShutdown args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnRefreshMovementSpeed(Entity<ActiveSandevistanComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.MovementSpeedModifier);
        args.CapSpeed(ent.Comp.MovementSpeedModifier);
    }

    private void OnGetMeleeAttackRate(Entity<ActiveSandevistanComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        if (args.User != ent.Owner || HasComp<GunComponent>(args.Weapon))
            return;

        args.Multipliers *= ent.Comp.AttackRateModifier;
    }
}
