// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Virus.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent, Access(typeof(SpawnAntagAfterSelectedRule))]
public sealed partial class SpawnAntagAfterSelectedComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId Prototype = "SentientVirus";
}
