using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.Arena;

[RegisterComponent]
public sealed partial class ArenaPlayerComponent : Component
{
    [DataField]
    public EntityUid? RuleEntity;
}
