// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Eui;
using Content.Shared.DeadSpace.Skills.Events;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client.DeadSpace.Skill;

[UsedImplicitly]
public sealed class SkillsListEui : BaseEui
{
    private readonly SkillsListWindow _window;

    public SkillsListEui()
    {
        _window = new SkillsListWindow();
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is SkillsListEuiState s)
        {
            _window.SetSkills(s.Skills, s.TargetName);
        }
    }

    public override void Closed()
    {
        _window.Close();
    }
}
