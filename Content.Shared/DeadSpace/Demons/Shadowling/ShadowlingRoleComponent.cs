using Content.Shared.Roles.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingRoleComponent : BaseMindRoleComponent { }

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingSlaveRoleComponent : BaseMindRoleComponent { }
