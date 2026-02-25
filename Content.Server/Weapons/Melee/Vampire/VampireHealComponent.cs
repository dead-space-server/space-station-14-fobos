using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared.FixedPoint;

namespace Content.Server.Weapons.Melee.Vampire;

[RegisterComponent]
public sealed partial class VampireHealComponent : Component
{
    [DataField("healMultiplier")]
    public FixedPoint2 HealMultiplier = 0.5f;
}