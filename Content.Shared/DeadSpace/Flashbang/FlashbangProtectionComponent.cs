// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Flashbang;

[RegisterComponent, NetworkedComponent]
public sealed partial class FlashbangProtectionComponent : Component
{
    [DataField]
    public float Reduction = 0f;
}
