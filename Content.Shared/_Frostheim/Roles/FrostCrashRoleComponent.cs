using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim.Roles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FrostCrashRoleComponent : Component
{
    [DataField, AutoNetworkedField]
    public string RoleName;

    [DataField, AutoNetworkedField]
    public string RoleDescription;

    [DataField, AutoNetworkedField]
    public string RoleRules;

    [DataField, AutoNetworkedField]
    public int MinPlayers;
}
