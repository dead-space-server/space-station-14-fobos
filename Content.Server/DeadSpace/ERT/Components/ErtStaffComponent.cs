// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.ERT.Components;

/// <summary>
/// Компонент для бойцов ERT отряда.
/// При добавлении mind им будет назначен objective с целью вызова.
/// </summary>
[RegisterComponent]
public sealed partial class ErtStaffComponent : Component
{
    /// <summary>
    /// Цель вызова ERT отряда.
    /// </summary>
    [DataField]
    public string? CallReason;
}
