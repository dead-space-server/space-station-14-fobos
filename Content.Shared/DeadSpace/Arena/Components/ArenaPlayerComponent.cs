using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.Arena;

[RegisterComponent]
public sealed partial class ArenaPlayerComponent : Component
{
    public EntityUid OriginalMind;
    public EntityUid OriginalGhost;
    public bool CanReturnToBody;
}
