// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Clothing.HideWhenSlotOccupied;

/// <summary>
/// Когда эта одежда будет надета, ее спрайтовые слои будут скрыты
/// если в указанном слоте будет надет другой предмет.
/// Слои снова станут видимыми, когда этот слот опустеет.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class HideWhenSlotOccupiedComponent : Component
{
    /// <summary>
    /// Название слота для просмотра. Когда предмет экипирован в этот слот,
    /// слои этой одежды будут скрыты.
    /// Examples: "outerClothing", "head", "mask", "eyes", "gloves", etc.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public string WatchedSlot = string.Empty;

    /// <summary>
    /// Сущность, которая в данный момент носит эту одежду.
    /// </summary>
    [ViewVariables]
    public EntityUid? Wearer;

    /// <summary>
    /// Являются ли слои в данный момент скрытыми из-за того, что просматриваемый слот занят.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool IsHidden;

    /// <summary>
    /// Являются ли слои в данный момент скрытыми из-за того, что просматриваемый слот занят.
    /// </summary>
    [DataField, ViewVariables]
    public EntityWhitelist? Whitelist = null;
}
