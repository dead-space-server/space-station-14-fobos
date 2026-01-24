// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.Revolutionary;

/// <summary>
/// Условие objective для революционеров - убить определённый процент командного состава.
/// Использует NumberObjectiveComponent для хранения целевого процента.
/// </summary>
[RegisterComponent, Access(typeof(KillCommandStaffConditionSystem))]
public sealed partial class KillCommandStaffConditionComponent : Component;
