// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using System.Collections.Generic;

namespace Content.Shared.DeadSpace.ConsoleCraft;

[RegisterComponent]
public sealed partial class CraftedItemModulesComponent : Component
{
    [DataField]
    public List<string> AppliedModules { get; set; } = new();
}
