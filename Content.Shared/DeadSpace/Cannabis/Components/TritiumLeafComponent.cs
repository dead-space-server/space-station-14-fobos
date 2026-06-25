using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Cannabis;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TritiumLeafComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Moles = 1;
}
