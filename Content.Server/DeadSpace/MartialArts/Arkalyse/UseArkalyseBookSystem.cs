using Content.Server.DeadSpace.MartialArts;
using Content.Server.DeadSpace.MartialArts.Components;
using Content.Shared.Interaction.Events;
using Content.Server.DeadSpace.MartialArts.SmokingCarp;
using Content.Shared.DeadSpace.MartialArts.Arkalyse;
using Content.Shared.Weapons.Melee;
using Content.Shared.Mobs.Components;
using Robust.Shared.Timing;
using Robust.Shared.Physics.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Actions;

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

        foreach (var actionId in userArkalyse.Comp.BaseArkalyse)
            _actionSystem.AddAction(args.User, actionId);

        if (TryComp<MeleeWeaponComponent>(args.User, out var melee))
            melee.AttackRate = ent.Comp.AddAtackRate;

        TransformToItem(ent, ent.Comp.ItemAfterLerning);

        Del(ent);
        Spawn(ent.Comp.ItemAfterLerning, _transform.GetMapCoordinates(ent));

        args.Handled = true;
        Dirty(userArkalyse);
    }
}
