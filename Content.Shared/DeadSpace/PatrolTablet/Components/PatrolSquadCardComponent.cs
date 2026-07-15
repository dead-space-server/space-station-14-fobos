using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.PatrolTablet;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PatrolSquadCardComponent : Component
{
    [DataField, AutoNetworkedField]
    public string SquadId = string.Empty;

    [DataField, AutoNetworkedField]
    public string SquadIcon = string.Empty;

    [DataField, AutoNetworkedField]
    public string MemberName = string.Empty;
}
