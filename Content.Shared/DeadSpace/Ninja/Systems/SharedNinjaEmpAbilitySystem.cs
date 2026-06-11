using Content.Shared.Emp;
using Content.Shared.Actions;
using Content.Shared.DeadSpace.Ninja.Components;

namespace Content.Shared.DeadSpace.Ninja.Systems;

public abstract class SharedNinjaEmpAbilitySystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedEmpSystem _emp = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NinjaEmpAbilityComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<NinjaEmpAbilityComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<NinjaEmpAbilityComponent, NinjaEmpEvent>(OnEmp);
    }

    private void OnCompInit(Entity<NinjaEmpAbilityComponent> ent, ref ComponentInit args)
    {
        var (uid, comp) = ent;
        _actionContainer.EnsureAction(uid, ref comp.EmpActionEntity, comp.EmpAction);
        Dirty(uid, comp);
    }

    private void OnGetActions(Entity<NinjaEmpAbilityComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;
        args.AddAction(ent.Comp.EmpActionEntity);
    }

    private void OnEmp(Entity<NinjaEmpAbilityComponent> ent, ref NinjaEmpEvent args)
    {
        args.Handled = true;
        var (uid, comp) = ent;
        _emp.EmpPulse(Transform(uid).Coordinates, comp.EmpRange, comp.EmpConsumption, comp.EmpDuration, args.Performer);
    }
}
