// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.ERT;

namespace Content.Server.DeadSpace.ERTCall;

[RegisterComponent, Access(typeof(ErtResponceSystem))]
public sealed partial class ErtSpeciesRoleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public WaitingSpeciesSettings? Settings;
}
