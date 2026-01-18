// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.Medieval.Skills.Prototypes;
using Content.Shared.DeadSpace.Medieval.Skills.Components;
using Content.Server.Popups;
using System.Linq;

namespace Content.Server.DeadSpace.Medieval.Skill;

public abstract class NeededSkillSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SkillSystem _skillSystem = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        _sawmill = Logger.GetSawmill("NeededSkillSystem");
    }

    public bool CheckRequiredSkills(EntityUid user, List<ProtoId<SkillPrototype>> neededSkills)
    {
        if (!TryComp<SkillComponent>(user, out var skillComponent))
            return true;

        var missingSkills = new List<string>();

        foreach (var skill in neededSkills)
        {
            if (!_prototypeManager.TryIndex(skill, out var skillPrototype) || skillPrototype == null)
            {
                _sawmill.Warning($"Прототип навыка {skill} не найден");
                continue;
            }

            if (!_skillSystem.CnowThisSkill(user, skill, skillComponent))
                missingSkills.Add(skillPrototype.Name);
        }

        if (missingSkills.Count > 0)
        {
            var skillsText = string.Join(", ", missingSkills.Select(s => Loc.GetString($"{s}")));
            var message = Loc.GetString("skill-need-skill-message", ("skills", skillsText));

            _popup.PopupEntity(message, user, user);
            return false;
        }

        return true;
    }


}
