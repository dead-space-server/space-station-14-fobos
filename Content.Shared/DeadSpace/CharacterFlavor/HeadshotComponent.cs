using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.CharacterFlavor;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HeadshotComponent : Component
{
    [AutoNetworkedField]
    public string HeadshotData = string.Empty;

    [AutoNetworkedField]
    public string FlavorText = string.Empty;
}
