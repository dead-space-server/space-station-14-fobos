using System.Numerics;
using Content.Shared.Item;
using Robust.Shared.Prototypes;

namespace Content.Shared.Kitchen.Components;

[RegisterComponent]
public sealed partial class PlateComponent : Component
{
    [DataField("slotId")]
    public string SlotId = "plate_slot";

    [DataField("contentOffset")]
    public Vector2 ContentOffset = Vector2.Zero;

    [DataField("heldContentOffsetLeft")]
    public Vector2 HeldContentOffsetLeft = Vector2.Zero;

    [DataField("heldContentOffsetRight")]
    public Vector2 HeldContentOffsetRight = Vector2.Zero;

    [DataField("contentScale")]
    public Vector2 ContentScale = Vector2.One;

    [DataField("maxItemSize")]
    public ProtoId<ItemSizePrototype> MaxItemSize = "Normal";
}
