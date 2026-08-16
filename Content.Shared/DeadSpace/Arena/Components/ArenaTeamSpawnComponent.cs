using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.Arena;

[RegisterComponent]
public sealed partial class ArenaTeamSpawnComponent : Component
{
    [DataField]
    public ArenaTeam Team = ArenaTeam.None;
}
