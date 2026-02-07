// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.ERT.Prototypes;
using Content.Shared.Mobs;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.ERT.Components;

[RegisterComponent]
public sealed partial class ResponceErtOnAllowedStateComponent : Component
{
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<ErtTeamPrototype> Team;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<MobState> AllowedStates = new();

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool IsReady = false;

    [DataField(required: true)]
    public EntProtoId ActionPrototype = "ActionCallErtHelp";

    [DataField]
    public EntityUid? ActionEntity;
}
