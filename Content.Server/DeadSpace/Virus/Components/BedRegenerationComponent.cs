// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class BedRegenerationComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public BedRegenerationType RegenerationType = BedRegenerationType.Normal;
}
