// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Content.Server.DeadSpace.MartialArts.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Melee;
using Content.Shared.Actions;
using Content.Server.DeadSpace.MartialArts.SmokingCarp.Components;
using Content.Server.DeadSpace.MartialArts.Arkalyse.Components;
using Robust.Server.GameObjects;

namespace Content.Server.DeadSpace.MartialArts;

public sealed class UseArkalyseBookSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
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
        userArkalyse.Params = ent.Comp.Params[0];

        foreach (var actionId in userArkalyse.BaseArkalyse)
            _action.AddAction(args.User, actionId);

        if (TryComp<MeleeWeaponComponent>(args.User, out var melee))
            melee.AttackRate = ent.Comp.AddAtackRate;

        Del(ent);
        Spawn(ent.Comp.ItemAfterLerning, _transform.GetMapCoordinates(args.User));

        args.Handled = true;
    }
}