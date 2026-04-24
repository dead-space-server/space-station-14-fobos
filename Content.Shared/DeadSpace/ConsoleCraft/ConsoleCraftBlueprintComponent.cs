// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.ConsoleCraft;

[RegisterComponent]
public sealed partial class ConsoleCraftBlueprintComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ConsoleCraftPrototype> Recipe { get; set; } = default!;

    [DataField]
    public int? MaxCrafts { get; set; } = null;

    [ViewVariables]
    public int CraftsUsed { get; set; } = 0;

    public int? RemainingCrafts => MaxCrafts.HasValue ? MaxCrafts.Value - CraftsUsed : null;

    public bool IsExhausted => MaxCrafts.HasValue && CraftsUsed >= MaxCrafts.Value;
}
