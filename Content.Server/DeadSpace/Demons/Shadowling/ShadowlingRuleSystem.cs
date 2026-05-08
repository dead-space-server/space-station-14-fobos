// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Antag;
using Content.Server.Database;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Prototypes;
using Content.Server.Mind;
using Content.Shared.Mobs.Components;
using Content.Server.GameTicking;
using Content.Shared.Mind.Components;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly IServerDbManager _db = default!;

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

    protected override void AppendRoundEndText(EntityUid uid,
        ShadowlingRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        if (component.ManifestWritten)
            return;
        component.ManifestWritten = true;

        var sessionData = _antag.GetAntagIdentifiers(uid);

        if (sessionData.Count == 0)
            return;

        args.AddLine(Loc.GetString("shadowling-round-end-count", ("initialCount", sessionData.Count)));

        foreach (var (mind, data, name) in sessionData)
        {
            var count = 0;
            if (_role.MindHasRole<ShadowlingRoleComponent>(mind, out var role))
                count = role.Value.Comp2.TotalRecruited;

            args.AddLine(Loc.GetString("shadowling-round-end-name-user",
                ("name", name),
                ("username", data.UserName),
                ("count", count)));
        }

        args.AddLine("");

        if (component.IsAscended)
            args.AddLine(Loc.GetString("shadowling-win"));
        else if (component.AllDead)
            args.AddLine(Loc.GetString("shadowling-lose"));
        else
            args.AddLine(Loc.GetString("shadowling-stalemate"));

        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                BiStatWinner winner;
                if (component.IsAscended)
                    winner = BiStatWinner.Antagonist;
                else if (component.AllDead)
                    winner = BiStatWinner.Crew;
                else
                    winner = BiStatWinner.Crew;

                await _db.AddBiStatAsync("Тенеморф", winner, DateTime.UtcNow);
            }
            catch { }
        });
    }

    protected override void ActiveTick(EntityUid uid, ShadowlingRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.IsAscended || component.AllDead)
            return;

        var sessionData = _antag.GetAntagIdentifiers(uid);
        var sessionUserIds = new HashSet<string>();

        foreach (var (mind, data, name) in sessionData)
        {
            sessionUserIds.Add(data.UserId.ToString());
        }

        if (sessionUserIds.Count == 0)
            return;

        var deadCount = 0;

        var entities = EntityQueryEnumerator<ShadowlingRecruitComponent, MindContainerComponent, MobStateComponent>();
        while (entities.MoveNext(out var entity, out var recruit, out var mindContainer, out var mob))
        {
            if (!_mind.TryGetMind(entity, out _, out var mind, mindContainer))
                continue;

            var userId = mind.UserId?.ToString() ?? mind.OriginalOwnerUserId?.ToString();
            if (userId == null || !sessionUserIds.Contains(userId))
                continue;

            component.HadShadowlings = true;

            if (!_mobState.IsAlive(entity, mob) && !HasComp<ShadowlingComponent>(entity) && !HasComp<ShadowlingAnnihilationComponent>(entity))
                deadCount++;
        }

        var hiddenEntities = EntityQueryEnumerator<ShadowlingRevealComponent, MindContainerComponent, MobStateComponent>();
        while (hiddenEntities.MoveNext(out var entity, out var reveal, out var mindContainer, out var mob))
        {
            if (!_mind.TryGetMind(entity, out _, out var mind, mindContainer))
                continue;

            var userId = mind.UserId?.ToString() ?? mind.OriginalOwnerUserId?.ToString();
            if (userId == null || !sessionUserIds.Contains(userId))
                continue;

            component.HadShadowlings = true;

            if (!_mobState.IsAlive(entity, mob))
                deadCount++;
        }

        if (component.HadShadowlings && deadCount >= sessionUserIds.Count)
            component.AllDead = true;
    }
}