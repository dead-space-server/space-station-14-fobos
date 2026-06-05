using System.Numerics;

namespace Content.Server.DeadSpace.Items;

[RegisterComponent]
public sealed partial class VehiclePushbackComponent : Component
{
    [DataField]
    public Vector2 ImpulsePerTick;

    [DataField]
    public int TicksLeft = 5;
}
