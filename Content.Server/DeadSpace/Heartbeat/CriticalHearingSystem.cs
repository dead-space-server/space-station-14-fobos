using Content.Server.Radio;
using Content.Shared.DeadSpace.Heartbeat;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Server.DeadSpace.Heartbeat;

public sealed class CriticalHearingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadioReceiveAttemptEvent>(OnRadioReceiveAttempt);
    }

    private void OnRadioReceiveAttempt(ref RadioReceiveAttemptEvent args)
    {
        var listener = args.RadioReceiver;

        for (var i = 0; i < 4; i++)
        {
            if (HasComp<CritHeartbeatComponent>(listener) &&
                TryComp<MobStateComponent>(listener, out var mobState) &&
                mobState.CurrentState is MobState.PreCritical or MobState.Critical)
            {
                args.Cancelled = true;
                return;
            }

            // DS14: walking up the parent chain can reach a root entity (map/grid) whose parent
            // is EntityUid.Invalid, which has no TransformComponent at all - Transform(listener)
            // throws in that case instead of returning nothing. Bail out gracefully instead of
            // crashing the whole server on what is otherwise an entirely ordinary radio message.
            if (!TryComp(listener, out TransformComponent? xform))
                return;

            var parent = xform.ParentUid;
            if (parent == listener || !parent.IsValid())
                return;

            listener = parent;
        }
    }
}
