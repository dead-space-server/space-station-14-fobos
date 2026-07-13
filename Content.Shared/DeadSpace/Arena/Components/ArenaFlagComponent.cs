using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Arena.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ArenaFlagComponent : Component
{
    [DataField, AutoNetworkedField]
    public ArenaTeam Team = ArenaTeam.None;

    [ViewVariables, AutoNetworkedField]
    public float CaptureProgress;

    [ViewVariables, AutoNetworkedField]
    public int CapturersCount;
}
