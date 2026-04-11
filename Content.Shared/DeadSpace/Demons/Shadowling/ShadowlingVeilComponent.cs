using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingVeilComponent : Component
{
    [ViewVariables] public float VeilTimer = 0f;
    [ViewVariables] public bool VeilActive = false;
    [ViewVariables] public List<EntityUid> AffectedLights = new();
}