// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Shared.DeadSpace.Movement.Components;

/// <summary>
/// Marker for hovering vehicles that should not be slowed by floor-contact fluids.
/// </summary>
[RegisterComponent]
public sealed partial class HoverIgnoresContactSlowdownComponent : Component;
