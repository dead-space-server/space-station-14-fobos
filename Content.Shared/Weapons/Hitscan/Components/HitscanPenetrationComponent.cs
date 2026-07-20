using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Hitscan.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HitscanPenetrationComponent : Component
{
    /// <summary>
    /// Maximum total destroyed-threshold damage this hitscan may penetrate through.
    /// </summary>
    [DataField]
    public FixedPoint2 PenetrationThreshold = FixedPoint2.Zero;

    /// <summary>
    /// If set, every listed damage type must be present in the dealt damage for penetration.
    /// </summary>
    [DataField]
    public List<string>? PenetrationDamageTypeRequirement;

    [DataField]
    public FixedPoint2 PenetrationAmount = FixedPoint2.Zero;

    // DS14-start
    /// <summary>
    /// Maximum number of hit entities the ray may pass through.
    /// </summary>
    [DataField]
    public int MaxPenetratedTargets = int.MaxValue;

    /// <summary>
    /// Maximum number of nearby destructible obstacles removed at each penetrated hit.
    /// </summary>
    [DataField]
    public int MaxDestroyedObstacles;

    [DataField]
    public float ObstacleSearchRadius = 0.25f;

    public int PenetratedTargets;
    // DS14-end
}
