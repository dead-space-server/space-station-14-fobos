using Content.Server.DeadSpace.MartialArts;
using Content.Shared.DeadSpace.MartialArts.SmokingCarp;
using Content.Server.DeadSpace.MartialArts.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Weapons.Reflect;
using Content.Server.DeadSpace.MartialArts.Arkalyse;

namespace Content.Server.DeadSpace.MartialArts.SmokingCarp;

[Dependency] private readonly SharedActionsSystem _actionSystem = default!;
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
            _actionSystem.AddAction(args.User, actionId);

        if (TryComp<MeleeWeaponComponent>(args.User, out var melee))
            melee.AttackRate = ent.Comp.AddAtackRate;

        Del(ent);
        Spawn(ent.Comp.ItemAfterLerning, _transform.GetMapCoordinates(ent));
        
        args.Handled = true;
        Dirty(userSmokingCarp);
    }
