using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Server.Mobs;

public sealed class CritActionCooldownSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<IgnoreCritCooldownComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<IgnoreCritCooldownComponent, ComponentInit>(OnCompInit);
    }

    private void OnMobStateChanged(Entity<IgnoreCritCooldownComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Critical || args.NewMobState == MobState.Dead)
            RemoveCritActionCooldowns(ent);
    }

    private void OnCompInit(Entity<IgnoreCritCooldownComponent> ent, ref ComponentInit args)
    {
        if (_mobState.IsCritical(ent) || _mobState.IsDead(ent))
            RemoveCritActionCooldowns(ent);
    }

    private void RemoveCritActionCooldowns(EntityUid uid)
    {
        if (!TryComp<ActionsComponent>(uid, out var actionsComp))
            return;

        foreach (var actionId in actionsComp.Actions)
        {
            if (!TryComp<ActionComponent>(actionId, out var actionComp))
                continue;

            if (actionComp.UseDelay == null)
                continue;

            if (!TryComp<InstantActionComponent>(actionId, out var instant))
                continue;

            if (instant.Event is CritSuccumbEvent or CritLastWordsEvent or CritFakeDeathEvent)
            {
                _actions.SetUseDelay((actionId, actionComp), null);
                _actions.RemoveCooldown((actionId, actionComp));
            }
        }
    }
}
