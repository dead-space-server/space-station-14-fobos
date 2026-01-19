// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Skill.Components;
using Content.Shared.DeadSpace.Skills.Events;

namespace Content.Server.DeadSpace.Skill;

public sealed class NeededSkillForInteractUsingSystem : EntitySystem
{
    [Dependency] private readonly SkillSystem _skillSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeededSkillForInteractUsingComponent, BeforeInteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, NeededSkillForInteractUsingComponent component, BeforeInteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_skillSystem.CheckRequiredSkills(args.User, component.NeededSkills))
            args.Handled = true;
    }

}
