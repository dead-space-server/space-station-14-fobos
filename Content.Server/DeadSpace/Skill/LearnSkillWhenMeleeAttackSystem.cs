// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Skill.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;

namespace Content.Server.DeadSpace.Skill;

public sealed class LearnSkillWhenMeleeAttackSystem : EntitySystem
{
    [Dependency] private readonly SkillSystem _skillSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LearnSkillWhenMeleeAttackComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(EntityUid uid, LearnSkillWhenMeleeAttackComponent component, MeleeHitEvent args)
    {
        foreach (var entity in args.HitEntities)
        {
            if (args.User == entity)
                continue;

            if (component.Whitelist != null)
            {
                if (_entityWhitelist.IsValid(component.Whitelist, entity))
                {
                    foreach (var skillDict in component.Points)
                    {
                        foreach (var (skill, value) in skillDict)
                        {
                            if (_skillSystem.CanLearn(args.User, skill))
                                _skillSystem.AddSkillProgress(args.User, skill, value);
                        }
                    }
                }
            }
            else
            {
                foreach (var skillDict in component.Points)
                {
                    foreach (var (skill, value) in skillDict)
                    {
                        if (_skillSystem.CanLearn(args.User, skill))
                            _skillSystem.AddSkillProgress(args.User, skill, value);
                    }
                }
            }
        }
    }

}
