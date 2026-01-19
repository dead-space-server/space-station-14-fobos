// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Skills.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Skill.Components;

[RegisterComponent]
public sealed partial class NeededSkillForInteractComponent : Component
{
    /// <summary>
    ///     Требуемые навыки
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<SkillPrototype>> NeededSkills;
}
