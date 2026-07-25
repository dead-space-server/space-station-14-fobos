// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Guardian;
using Content.Shared.DeadSpace.MovementLimit;
using System;

namespace Content.Server.DeadSpace.Guardian;

public sealed class GuardianDistanceVisualsSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GuardianComponent>();
        while (query.MoveNext(out var uid, out var guardian))
        {
            if (guardian.Host == null || TerminatingOrDeleted(guardian.Host.Value))
            {
                if (HasComp<DistanceLimitVisualsComponent>(uid))
                    RemComp<DistanceLimitVisualsComponent>(uid);
                continue;
            }

            var visuals = EnsureComp<DistanceLimitVisualsComponent>(uid);
            var isDirty = false;

            if (visuals.Origin != guardian.Host)
            {
                visuals.Origin = guardian.Host;
                isDirty = true;
            }

            if (Math.Abs(visuals.MaxDistance - guardian.DistanceAllowed) > 0.01f)
            {
                visuals.MaxDistance = guardian.DistanceAllowed;
                isDirty = true;
            }

            if (isDirty)
                Dirty(uid, visuals);
        }
    }
}
