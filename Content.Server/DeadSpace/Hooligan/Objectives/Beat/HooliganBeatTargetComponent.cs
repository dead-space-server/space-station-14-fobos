// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.Hooligan.Objectives;

/// <summary>
/// Цель-жертва и Атакующий. Атакующий должен нанести по цели определённое количество урона
/// Цель-жертва будет засчитывать урон только от атакующего
/// </summary>
[RegisterComponent, Access(typeof(HooliganBeatConditionSystem))]
public sealed partial class HooliganBeatTargetComponent : Component
{
    public EntityUid Objective; // Цель-жертва атакующего

    public EntityUid Attacker; // Атакующий. Тот, кто наносит урон.
}
