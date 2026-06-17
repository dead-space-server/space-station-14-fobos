using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Client-side fade for a spent casing before the server despawns it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CasingFadeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float FadeDelay = 4f;

    [DataField, AutoNetworkedField]
    public float FadeDuration = 1.5f;
}
