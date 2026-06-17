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
}
