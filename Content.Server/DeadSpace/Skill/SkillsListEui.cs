// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.EUI;
using Content.Shared.DeadSpace.Skills;
using Content.Shared.DeadSpace.Skills.Events;
using Content.Shared.Eui;

namespace Content.Server.DeadSpace.Skill;

public sealed class SkillsListEui : BaseEui
{
    private readonly string _targetName;
    private readonly List<SkillInfo> _skills;

    public SkillsListEui(string targetName, List<SkillInfo> skills)
    {
        _targetName = targetName;
        _skills = skills;
    }

    public override void Opened()
    {
        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        return new SkillsListEuiState(_targetName, _skills);
    }
}
