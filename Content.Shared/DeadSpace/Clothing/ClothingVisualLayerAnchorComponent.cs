// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Clothing;

/// <summary>
/// Inserts this clothing item's equipped visuals after another inventory slot's visuals.
/// Use this for exceptional sprites that need to render above their normal slot order.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClothingVisualLayerAnchorComponent : Component
{
    /// <summary>
    /// Inventory slot layer to use as the visual insertion anchor.
    /// Examples: "head", "mask", "neck".
    /// </summary>
    [DataField("slot", required: true), AutoNetworkedField]
    public string Slot = string.Empty;
}
