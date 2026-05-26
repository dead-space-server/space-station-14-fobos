// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Shared.DeadSpace.Chemistry.Components;

/// <summary>
/// Prevents reagents from being injected into this entity.
/// </summary>
[RegisterComponent]
public sealed partial class InjectionBlockerComponent : Component
{
    [DataField]
    public LocId? BlockedMessage;
}
