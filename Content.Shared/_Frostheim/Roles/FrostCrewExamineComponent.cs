using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim.Roles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FrostCrewExamineComponent : Component
{
    [DataField, AutoNetworkedField]
    public string RoleLocKey = string.Empty;

    [DataField, AutoNetworkedField]
    public int MessageCount = 3;
}
