using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Lavaland.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LavalandMiningPointsComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Balance;
}
