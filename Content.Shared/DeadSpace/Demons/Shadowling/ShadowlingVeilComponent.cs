// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingVeilComponent : Component
{
    [ViewVariables] public float VeilTimer = 0f;
    [ViewVariables] public bool VeilActive = false;
    [ViewVariables] public List<EntityUid> AffectedLights = new();
}