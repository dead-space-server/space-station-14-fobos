// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.Necromorphs.Unitology.Components;

[RegisterComponent, Access(typeof(UnitologyAssemblyConditionSystem))]
public sealed partial class UnitologyAssemblyConditionComponent : Component
{
    [DataField]
    public float Range = 3f;
}
