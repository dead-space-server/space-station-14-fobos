// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.TimeWindow;

namespace Content.Server.DeadSpace.ERTCall;

[RegisterComponent]
public sealed partial class ErtComputerShuttleComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsEvacuation = false;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow EvacuationWindow = new(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextAnnounceTime = TimeSpan.Zero;
}
