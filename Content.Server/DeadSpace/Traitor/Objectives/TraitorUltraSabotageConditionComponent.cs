// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Traitor.Objectives;

[RegisterComponent, Access(typeof(TraitorUltraSabotageConditionSystem))]
public sealed partial class TraitorUltraSabotageConditionComponent : Component
{
    [DataField(required: true)]
    public List<TraitorUltraSabotageGroup> Groups = new();

    [DataField(required: true)]
    public LocId Title;

    [DataField(required: true)]
    public LocId Description;

    public EntityUid? TargetStation;

    public readonly List<TraitorUltraSabotageGroupState> GroupStates = new();
}

[DataDefinition]
public sealed partial class TraitorUltraSabotageGroup
{
    [DataField(required: true)]
    public List<EntProtoId> Prototypes = new();

    [DataField]
    public int Required;
}

public sealed class TraitorUltraSabotageGroupState
{
    public readonly List<EntityUid> Targets = new();
    public int Required;
}
