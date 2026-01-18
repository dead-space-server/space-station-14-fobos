// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Medieval.Skills.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Medieval.Skill.Components;

[RegisterComponent]
public sealed partial class LearnSkillWhenUsingComponent : Component
{
    /// <summary>
    ///     Изучаемые навыки навыки
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<SkillPrototype>> Skills;

    /// <summary>
    ///     Количество даваемых очков при изучении
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<SkillPrototype>, float> Points { get; set; } = new Dictionary<ProtoId<SkillPrototype>, float>();

    /// <summary>
    ///     Длительность изучения (секунд)
    /// </summary>
    [DataField]
    public float Duration;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? Sound = default!;
}
