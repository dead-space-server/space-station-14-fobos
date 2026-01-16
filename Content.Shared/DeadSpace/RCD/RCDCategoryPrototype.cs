using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.RCD;

/// <summary>
/// Defines a category for RCD operations in the radial menu
/// </summary>
[Prototype("rcdCategory")]
public sealed class RCDCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Localization key for the tooltip/name of this category
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public string Tooltip { get; private set; } = default!;

    /// <summary>
    /// Sprite specifier for the category icon in the radial menu
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public SpriteSpecifier Sprite { get; private set; } = default!;
}
