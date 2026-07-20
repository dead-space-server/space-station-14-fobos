// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Traitor.Objectives;

[RegisterComponent, Access(typeof(TraitorUltraKillDepartmentConditionSystem))]
public sealed partial class TraitorUltraKillDepartmentConditionComponent : Component
{
    [DataField(required: true)]
    public ProtoId<DepartmentPrototype> Department;

    [DataField]
    public float TargetFraction = 0.5f;

    [DataField]
    public int MinTargets = 2;

    [DataField(required: true)]
    public LocId Title;

    [DataField(required: true)]
    public LocId Description;

    public readonly List<EntityUid> TargetMinds = new();
    public int RequiredKills;
}
