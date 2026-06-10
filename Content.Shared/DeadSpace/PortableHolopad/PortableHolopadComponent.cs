// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Server.DeadSpace.PortableHolopad;

[RegisterComponent, NetworkedComponent]
public sealed partial class PortableHolopadComponent : Component
{
    [DataField]
    public bool Deployed = false;
}