using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim.Roles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FrostCrashRoleComponent : Component
{
    [DataField, AutoNetworkedField]
    public string RoleName = string.Empty;

    [DataField, AutoNetworkedField]
    public string RoleDescription = string.Empty;

    [DataField, AutoNetworkedField]
    public string RoleRules = string.Empty;

    [DataField, AutoNetworkedField]
    public int MinPlayers;
}
