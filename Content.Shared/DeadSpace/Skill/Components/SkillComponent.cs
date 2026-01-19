// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Skills.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Skills.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SkillComponent : Component
{
    /// <summary>
    ///     Группа навыков
    /// </summary>
    [DataField]
    public ProtoId<SkillGroupPrototype>? Group;

    /// <summary>
    ///     ID навыка и процент его изученности от 0 до 1
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<SkillPrototype>, float> Skills { get; set; } = new Dictionary<ProtoId<SkillPrototype>, float>();
}


