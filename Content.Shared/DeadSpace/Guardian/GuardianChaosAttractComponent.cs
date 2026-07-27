using Content.Shared.Physics;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Guardian;

/// <summary>
/// Pulls nearby entities on melee hit while ignoring the guardian host.
/// </summary>
[RegisterComponent]
public sealed partial class GuardianChaosAttractComponent : Component
{
    /// <summary>
    /// Pull speed. Negative values attract toward the guardian.
    /// </summary>
    [DataField]
    public float Speed = -15f;

    /// <summary>
    /// Maximum pull range.
    /// </summary>
    [DataField]
    public float Range = 5f;

    /// <summary>
    /// Entities affected by the pull.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Collision layers excluded from the pull.
    /// </summary>
    [DataField]
    public CollisionGroup CollisionMask = CollisionGroup.GhostImpassable;
}
