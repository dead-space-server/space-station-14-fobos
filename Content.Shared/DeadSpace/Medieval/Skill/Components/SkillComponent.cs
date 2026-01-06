// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Medieval.Skills.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SkillComponent : Component
{
    /// <summary>
    ///     ID навыка и процент его изученности от 0 до 1
    /// </summary>
    [DataField]
    public Dictionary<string, float> Skills { get; set; } = new Dictionary<string, float>();

    /// <summary>
    ///     Стандартное значение лимита
    /// </summary>
    [DataField]
    public int DefaultMaxLimit = 3;

    /// <summary>
    ///     Лимиты под определённые навыки
    /// </summary>
    [DataField]
    public Dictionary<string, int> MaxLimit { get; set; } = new Dictionary<string, int>();
}


