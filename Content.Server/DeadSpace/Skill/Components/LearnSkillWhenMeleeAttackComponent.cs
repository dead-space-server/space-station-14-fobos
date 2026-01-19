// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Skills.Prototypes;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Skill.Components;

[RegisterComponent]
public sealed partial class LearnSkillWhenMeleeAttackComponent : Component
{
    /// <summary>
    ///     Количество даваемых очков при изучении
    /// </summary>
    [DataField(required: true)]
    public List<Dictionary<ProtoId<SkillPrototype>, float>> Points { get; set; } = new();

    /// <summary>
    ///     Кого нужно бить чтобы навык прокачивался
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = null;
}
