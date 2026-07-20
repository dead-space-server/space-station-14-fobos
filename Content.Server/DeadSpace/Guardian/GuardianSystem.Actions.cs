#pragma warning disable IDE0130 // Namespace does not match folder structure
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Guardian;
using Content.Shared.Movement.Components;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Server.Guardian;

public sealed partial class GuardianSystem
{
    partial void OnGuardianLooseChanged(EntityUid guardian, GuardianComponent guardianComponent)
    {
        if (TryComp<ActionGrantComponent>(guardian, out var actionGrant))
        {
            foreach (var action in actionGrant.ActionEntities)
            {
                if (IsGuardianToggleAction(action))
                {
                    _actionSystem.SetEnabled(action, true);
                    continue;
                }

                _actionSystem.SetEnabled(action, guardianComponent.GuardianLoose);
            }
        }

        if (TryComp<ActionGunComponent>(guardian, out var actionGun))
            _actionSystem.SetEnabled(actionGun.ActionEntity, guardianComponent.GuardianLoose);

        if (TryComp<JumpAbilityComponent>(guardian, out var jumpAbility))
            _actionSystem.SetEnabled(jumpAbility.ActionEntity, guardianComponent.GuardianLoose);
    }

    private bool IsGuardianToggleAction(EntityUid action)
    {
        return TryComp<InstantActionComponent>(action, out var instantAction)
            && instantAction.Event is GuardianToggleActionEvent;
    }
}
