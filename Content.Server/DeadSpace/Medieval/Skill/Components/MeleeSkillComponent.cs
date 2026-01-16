// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Medieval.Skills.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Medieval.Skill.Components;

[RegisterComponent]
public sealed partial class MeleeSkillComponent : Component
{
    /// <summary>
    ///     Множитель без какого-либо навыка
    /// </summary>
    [DataField]
    public float DefaultModifier = 2f;

    /// <summary>
    ///     Требуемые навыки
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<SkillPrototype>> Skills;

    /// <summary>
    ///     Множители урона на навык (будет браться максимальный из всех)
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<SkillPrototype>, float> DamageModifiers { get; set; } = new Dictionary<ProtoId<SkillPrototype>, float>();

}
