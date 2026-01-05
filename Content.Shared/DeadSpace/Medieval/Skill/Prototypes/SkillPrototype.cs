// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.Medieval.Skills.Prototypes;

[Prototype("skill")]
public sealed partial class SkillPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = string.Empty;

    [DataField]
    public string Description = string.Empty;

    /// <summary>
    ///     Сколько места занимает навык в лимите
    /// </summary>
    [DataField]
    public Dictionary<string, int> AddLimit { get; set; } = new Dictionary<string, int>();

    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new("/Textures/_DeadSpace/Sith/actions/submission.png"));

    /// <summary>
    ///     Требуемые навыки для изучения этого навыка
    /// </summary>
    [DataField]
    public List<string>? RequiredSkills;

    /// <summary>
    ///     Количество баллов требуемое для изучения этого навыка
    /// </summary>
    [DataField]
    public int PointsNeeded = 1;

}
