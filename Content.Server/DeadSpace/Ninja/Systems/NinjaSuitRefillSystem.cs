using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Interaction;
using Content.Shared.Charges.Systems;
using Content.Shared.Charges.Components;
using Content.Shared.Stacks;
using Content.Shared.Popups;

namespace Content.Server.DeadSpace.Ninja.Systems;

public sealed class NinjaSuitRefillSystem : EntitySystem
{
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NinjaSuitRefillComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(Entity<NinjaSuitRefillComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // Проверяем что предмет это стак
        if (!TryComp<StackComponent>(args.Used, out var stack))
            return;

        if (!TryComp<NinjaSuitActionsComponent>(ent, out var suitActions))
            return;

        foreach (var (actionProtoId, cost) in ent.Comp.ActionMaterials)
        {
            // Проверяем что тип стака совпадает
            if (stack.StackTypeId != cost.Stack)
                continue;

            // Ищем EntityUid действия
            EntityUid? actionUid = null;
            for (var i = 0; i < suitActions.Actions.Count; i++)
            {
                if (suitActions.Actions[i] != actionProtoId)
                    continue;

                if (i < suitActions.ActionEntities.Count)
                    actionUid = suitActions.ActionEntities[i];

                break;
            }

            if (actionUid == null)
                continue;

            if (!TryComp<LimitedChargesComponent>(actionUid, out var charges))
                continue;

            if (charges.LastCharges >= charges.MaxCharges)
                continue;

            if (stack.Count < cost.Amount)
                continue;

            if (!_stack.TryUse(args.Used, cost.Amount))
                continue;

            _charges.AddCharges((actionUid.Value, charges), 1);
            _popup.PopupEntity(Loc.GetString("ninja-action-refill", ("action", MetaData(actionUid.Value).EntityName)), args.User, args.User, PopupType.Small);
            args.Handled = true;
            break;
        }
    }
}