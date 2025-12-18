// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Roles.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class SpawnAntagAfterSelectedComponent : BaseMindRoleComponent
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId Prototype = "SentientVirus";
}
