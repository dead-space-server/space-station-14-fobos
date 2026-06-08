// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Collections.Generic;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Administration.GameRules;

[Prototype]
public sealed partial class GameRuleCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField(required: true)]
    public string Name { get; set; } = default!;

    [DataField]
    public int Order { get; set; }

    [DataField(required: true)]
    public List<string> Rules { get; set; } = new();
}