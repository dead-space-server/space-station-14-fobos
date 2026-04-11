using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Objectives;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Mobs.Systems;
using Content.Server.Mind;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingRecruitObjectiveSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingRecruitObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<ShadowlingRecruitObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnGetProgress(EntityUid uid, ShadowlingRecruitObjectiveComponent component, ref ObjectiveGetProgressEvent args)
    {
        if (args.Mind.OwnedEntity == null)
            return;

        var count = GetCount(args.Mind.OwnedEntity.Value, component);

        args.Progress = Math.Clamp((float) count / component.TargetCount, 0f, 1f);

        var description = Loc.GetString("shadowling-recruit-objective-desc",
            ("current", count),
            ("target", component.TargetCount));

        _metaData.SetEntityDescription(uid, description);
    }

    private void OnAfterAssign(EntityUid uid, ShadowlingRecruitObjectiveComponent component, ref ObjectiveAfterAssignEvent args)
    {
        _metaData.SetEntityName(uid, Loc.GetString("shadowling-recruit-objective-title"), args.Meta);

        int count = 0;
        if (args.Mind.OwnedEntity != null)
        {
            count = GetCount(args.Mind.OwnedEntity.Value, component);
        }

        _metaData.SetEntityDescription(uid, Loc.GetString("shadowling-recruit-objective-desc",
            ("current", count),
            ("target", component.TargetCount)), args.Meta);
    }

    private int GetCount(EntityUid master, ShadowlingRecruitObjectiveComponent component)
    {
        int count = 0;
        var query = EntityQueryEnumerator<ShadowlingSlaveComponent>();
        while (query.MoveNext(out var sUid, out var slave))
        {
            if (slave.Master == master && _mobState.IsAlive(sUid))
                count++;
        }
        return count;
    }
}
