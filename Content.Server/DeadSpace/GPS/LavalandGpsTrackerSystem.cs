using Content.Server.DeadSpace.GPS.Components;
using Content.Server.DeadSpace.Lavaland.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Map;

namespace Content.Server.DeadSpace.GPS;

public sealed class LavalandGpsTrackerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<LavalandGpsTrackerComponent>();

        while (query.MoveNext(out var uid, out var tracker))
        {
            if (now < tracker.NextBeepTime)
                continue;

            var gpsPos = _transform.GetMapCoordinates(uid);
            if (gpsPos.MapId == MapId.Nullspace)
                continue;

            var found = false;
            var minDistSq = float.MaxValue;
            var arenaQuery = EntityQueryEnumerator<LavalandBossArenaComponent>();

            while (arenaQuery.MoveNext(out var arenaUid, out var arena))
            {
                if (arena.Ended)
                    continue;

                var arenaPos = _transform.GetMapCoordinates(arenaUid);
                if (arenaPos.MapId != gpsPos.MapId)
                    continue;

                var distSq = (gpsPos.Position - arenaPos.Position).LengthSquared();
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    found = true;
                }
            }

            if (!found)
                continue;

            var distance = MathF.Sqrt(minDistSq);
            if (distance > tracker.DetectionRange)
                continue;

            var t = distance / tracker.DetectionRange;
            var intervalSeconds = tracker.MinBeepInterval + (tracker.MaxBeepInterval - tracker.MinBeepInterval) * t;
            tracker.NextBeepTime = now + TimeSpan.FromSeconds(intervalSeconds);

            _audio.PlayPvs(tracker.BeepSound, uid);
        }
    }
}
