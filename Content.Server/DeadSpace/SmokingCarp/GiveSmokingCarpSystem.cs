using Content.Server.DeadSpace.SleepyCarp;
using Content.Server.DeadSpace.SmokingCarp.Abilities.Components;
using Content.Shared.DeadSpace.SmokingCarp.Abilities.Components;
using Content.Shared.DeadSpace.SmokingCarp;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Robust.Server.GameObjects;
using Content.Shared.Weapons.Melee;

namespace Content.Server.DeadSpace.SmokingCarp
{
    public sealed class GiveSmokingCarpSystem : EntitySystem
    {
        [Dependency] private readonly TransformSystem _transform = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GiveSmokingCarpComponent, UseInHandEvent>(OnUseInHand);
            SubscribeLocalEvent<GiveSmokingCarpComponent, GiveSmokingCarpDoAfterEvent>(OnDoAfter);
        }

        private void OnUseInHand(EntityUid uid, GiveSmokingCarpComponent component, UseInHandEvent args)
        {
            if (args.Handled)
                return;

            if (HasComp<SmokePunchComponent>(args.User) || HasComp<ReflectCarpComponent>(args.User) || HasComp<PowerPunchComponent>(args.User) || HasComp<TripPunchComponent>(args.User))
                return;

            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, component.InjectTime, new GiveSmokingCarpDoAfterEvent(), uid)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = true
            };

            _doAfter.TryStartDoAfter(doAfterArgs);

            args.Handled = true;
        }

        private void OnDoAfter(EntityUid uid, GiveSmokingCarpComponent component, DoAfterEvent args)
        {
            if (args.Handled || args.Cancelled)
                return;

            args.Handled = true;

            var meleeWeaponComponent = EnsureComp<MeleeWeaponComponent>(args.Args.User);
            meleeWeaponComponent.AttackRate = 1.3f;
            meleeWeaponComponent.Animation = component.Animation;
            meleeWeaponComponent.HitSound = component.HitSound;

            EnsureComp<SmokePunchComponent>(args.Args.User);
            EnsureComp<ReflectCarpComponent>(args.Args.User);
            EnsureComp<PowerPunchComponent>(args.Args.User);
            EnsureComp<TripPunchComponent>(args.Args.User);
            EnsureComp<SmokingCarpNotRangeComponent>(args.Args.User);

            TransformToItem(uid, component);
        }

        private void TransformToItem(EntityUid item, GiveSmokingCarpComponent component)
        {
            var position = _transform.GetMapCoordinates(item);
            Del(item);
            Spawn("SmokingCarpInjectorAfter", position);
        }
    }
}
