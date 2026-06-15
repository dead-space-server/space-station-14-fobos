// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.EntityConditions.Conditions.Inventory;

/// <inheritdoc cref="EntityCondition"/>
public sealed partial class WearingFullHardsuitCondition : EntityConditionBase<WearingFullHardsuitCondition>
{
    [DataField]
    public string HeadSlot = "head";

    [DataField]
    public string OuterClothingSlot = "outerClothing";

    public override string EntityConditionGuidebookText(IPrototypeManager prototype) => string.Empty;
}
