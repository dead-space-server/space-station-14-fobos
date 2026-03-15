// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Content.Shared.Item;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Kitchen.Components;

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
