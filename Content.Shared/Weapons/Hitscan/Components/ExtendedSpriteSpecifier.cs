using System.Numerics;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Hitscan.Components;

// DS14-start: hitscan projectile visual data.
[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class ExtendedSpriteSpecifier
{
    [DataField("sprite", required: true)]
    public SpriteSpecifier Sprite = default!;

    [DataField("color")]
    public Color SpriteColor = Color.White;

    [DataField("scale")]
    public Vector2 SpriteScale = Vector2.One;

    [DataField("noRot")]
    public bool SpriteRotation = true;
}
// DS14-end
