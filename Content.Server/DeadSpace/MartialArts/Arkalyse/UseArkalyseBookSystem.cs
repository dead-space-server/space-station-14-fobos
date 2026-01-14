using Content.Server.DeadSpace.MartialArts.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Melee;
using Content.Shared.Actions;
using Content.Server.DeadSpace.MartialArts.SmokingCarp.Components;
using Content.Server.DeadSpace.MartialArts.Arkalyse.Components;

namespace Content.Server.DeadSpace.MartialArts;

public sealed class UseArkalyseBookSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MartialArtsTrainingArkalyseComponent, UseInHandEvent>(OnUseBookArkalyse);
    }
    private void OnUseBookArkalyse(Entity<MartialArtsTrainingArkalyseComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || TryComp<SmokingCarpComponent>(args.User, out _))
            return;

        var userArkalyse = EnsureComp<ArkalyseComponent>(args.User);
        userArkalyse.Params = ent.Comp.Params;

        foreach (var actionId in userArkalyse.BaseArkalyse)
            _actionSystem.AddAction(args.User, actionId);

        if (TryComp<MeleeWeaponComponent>(args.User, out var melee))
            melee.AttackRate = ent.Comp.AddAtackRate;

        Del(ent);
        Spawn(ent.Comp.ItemAfterLerning, _transform.GetMapCoordinates(ent));

        args.Handled = true;
    }
}
