using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Server.Mind;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public readonly EntProtoId ObjectiveId = "ShadowlingRecruitObjective";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingRevealComponent, MapInitEvent>(OnShadowlingInit);
    }

    private void OnShadowlingInit(EntityUid uid, ShadowlingRevealComponent component, MapInitEvent args)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        _mind.TryAddObjective(mindId, mind, ObjectiveId);
    }
}
