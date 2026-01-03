// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Virus.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SentientVirusComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId ShopMutationAbility = "ShopMutationAbility";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ShopMutationActionEntity;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId SelectPrimaryPatientAbility = "SelectPrimaryPatientAbility";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? SelectPrimaryPatientActionEntity;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId TeleportToPrimaryPatientAbility = "TeleportToPrimaryPatientAbility";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? TeleportToPrimaryPatientActionEntity;

    /// <summary>
    ///     Итерация выбранного первичного заражённого для телепорта.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int SelectedPrimaryInfected = 0;

    /// <summary>
    ///     Максимальное количество первичных заражённых.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int MaxPrimaryInfected = 3;

    /// <summary>
    ///     Текущие первичные заражённые.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> CurrentPrimaryInfected = new();

    /// <summary>
    ///     Сколько всего было первичных заражённых.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int FactPrimaryInfected = 0;

    /// <summary>
    ///     Окно времени обновления.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow UpdateWindow = new TimedWindow(TimeSpan.FromSeconds(2f), TimeSpan.FromSeconds(2f));

    /// <summary>
    ///     Данные об вирусе.
    /// </summary>
    [DataField]
    public VirusData? Data = null;
}
