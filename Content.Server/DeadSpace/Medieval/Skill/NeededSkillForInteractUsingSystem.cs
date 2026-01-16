// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Medieval.Skill.Components;
using Content.Shared.DeadSpace.Medieval.Skills.Events;

namespace Content.Server.DeadSpace.Medieval.Skill;

public sealed class NeededSkillForInteractUsingSystem : NeededSkillSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeededSkillForInteractUsingComponent, BeforeInteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, NeededSkillForInteractUsingComponent component, BeforeInteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!CheckRequiredSkills(args.User, component.NeededSkills))
            args.Handled = true;
    }

}
