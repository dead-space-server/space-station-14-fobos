// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Medieval.Skill.Components;
using Content.Shared.DeadSpace.Medieval.Skills.Events;
using Content.Shared.Interaction;

namespace Content.Server.DeadSpace.Medieval.Skill;

public sealed class NeededSkillForInteractSystem : NeededSkillSystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeededSkillForInteractComponent, BeforeInteractionActivate>(OnBeforeActivate);
        SubscribeLocalEvent<NeededSkillForInteractComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnBeforeActivate(EntityUid uid, NeededSkillForInteractComponent component, BeforeInteractionActivate args)
    {
        if (args.Handled)
            return;

        if (!CheckRequiredSkills(args.User, component.NeededSkills))
            args.Handled = true;
    }

    private void OnInteractHand(EntityUid uid, NeededSkillForInteractComponent component, InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!CheckRequiredSkills(args.User, component.NeededSkills))
            args.Handled = true;
    }

}
