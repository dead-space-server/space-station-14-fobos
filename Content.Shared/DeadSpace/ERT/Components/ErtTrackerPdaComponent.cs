// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.ERT.Components;

/// <summary>
/// Marks a PDA that can assign the ERT tracking target for its user.
/// </summary>
[RegisterComponent]
public sealed partial class ErtTrackerPdaComponent : Component
{
    [DataField]
    public HashSet<ProtoId<DepartmentPrototype>> AllowedDepartments = new();

    [DataField]
    public HashSet<ProtoId<JobPrototype>> AllowedJobs = new();
}
