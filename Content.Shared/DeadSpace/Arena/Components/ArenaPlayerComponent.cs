using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.Arena;

[RegisterComponent, NetworkedComponent]
public sealed partial class ArenaPlayerComponent : Component
{
    public EntityUid OriginalMind;
    public EntityUid OriginalGhost;
    public bool CanReturnToBody;
    public ArenaTeam Team = ArenaTeam.None;
}
