// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;

namespace Content.Shared.DeadSpace.Virus.Symptoms;

public interface IVirusSymptom
{
    VirusSymptom Type { get; }

    TimedWindow EffectTimedWindow { get; }

    /// <summary>
    ///     Вызывается при добавлении симптома.
    /// </summary>
    void OnAdded(EntityUid host, VirusComponent virus);

    /// <summary>
    ///     Периодически вызывается VirusSystem, для обновления симптома.
    /// </summary>
    void OnUpdate(EntityUid host, VirusComponent virus);

    /// <summary>
    ///     Вызывается при удалении симптома (например, излечение).
    /// </summary>
    void OnRemoved(EntityUid host, VirusComponent virus);

    /// <summary>
    ///     Запускает эффект симптома.
    /// </summary>
    void DoEffect(EntityUid host, VirusComponent virus);

    /// <summary>
    ///     Метод для передачи симптомов от одного носителя к другому.
    /// </summary>
    IVirusSymptom Clone();
}
