using Content.Server.DeadSpace.MartialArts.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Melee;
using Content.Shared.Actions;
using Content.Server.DeadSpace.MartialArts.Arkalyse.Components;
using Content.Server.DeadSpace.MartialArts.SmokingCarp.Components;
using Robust.Server.GameObjects;

namespace Content.Server.DeadSpace.MartialArts.SmokingCarp;
public sealed class UseArkalyseBookSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MartialArtsTrainingCarpComponent, UseInHandEvent>(OnUseInjectorSmokingCarp);
    }

    private void OnUseInjectorSmokingCarp(Entity<MartialArtsTrainingCarpComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || TryComp<ArkalyseComponent>(args.User, out _))
            return;

        EnsureComp<SmokingCarpTripPunchComponent>(args.User);
        var userSmokingCarp = EnsureComp<SmokingCarpComponent>(args.User);
        userSmokingCarp.Params = ent.Comp.Params;

        foreach (var actionId in userSmokingCarp.BaseSmokingCarp)
            _action.AddAction(args.User, actionId);

        if (TryComp<MeleeWeaponComponent>(args.User, out var melee))
            melee.AttackRate = ent.Comp.AddAtackRate;

        Del(ent);
        Spawn(ent.Comp.ItemAfterLerning, _transform.GetMapCoordinates(ent));

        args.Handled = true;
    }
}
