// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Cargo.Prototypes;
using Content.Shared.DeadSpace.ERT.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.ERTCall;

[RegisterComponent]
public sealed partial class ErtResponceConsoleComponent : Component
{
    [DataField]
    public List<ProtoId<ErtTeamPrototype>> Teams = new();

    [DataField]
    public ProtoId<CargoAccountPrototype> Account = "Cargo";
}
