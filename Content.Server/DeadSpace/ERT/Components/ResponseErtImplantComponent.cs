// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.ERT.Prototypes;
using Content.Shared.Mobs;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.ERT.Components;

[RegisterComponent, ComponentProtoName("ResponseErtImplant")]
public sealed partial class ResponseErtImplantComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ErtTeamPrototype> Team;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public List<MobState> AllowedStates = new();

    [DataField]
    public EntProtoId ActionPrototype = "ActionCallErtHelp";
}
