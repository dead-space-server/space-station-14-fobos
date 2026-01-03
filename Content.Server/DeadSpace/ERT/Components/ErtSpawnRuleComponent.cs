// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.ERT;
using Content.Shared.DeadSpace.ERT.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.ERTCall;

[RegisterComponent, Access(typeof(ErtResponceSystem))]
public sealed partial class ErtSpawnRuleComponent : Component
{
    public ProtoId<ErtTeamPrototype> Team;
}
