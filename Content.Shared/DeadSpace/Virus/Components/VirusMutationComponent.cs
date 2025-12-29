// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Virus.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VirusMutationComponent : Component
{
    /// <summary>
    ///     Дополнительный шанс мутации.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float AddMutationChance = 0.1f;

    /// <summary>
    ///     Минимальное время до следующей мутации.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float MinUpdateTime = 3f;

    /// <summary>
    ///     Максимальное время до следующей мутации.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float MaxUpdateTime = 60f;

    /// <summary>
    ///     Может ли существо очистить сущность от вируса.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool CanClear = false;

    /// <summary>
    ///     Нужно ли менять отображение сущности?
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool ChangeApperance = false;

    /// <summary>
    ///     Окно времени обновления мутации.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow? UpdateWindow;

    #region Visualizer

    [DataField]
    public string State = "icon";

    [DataField]
    public string InfectedState = "infected";

    #endregion
}
