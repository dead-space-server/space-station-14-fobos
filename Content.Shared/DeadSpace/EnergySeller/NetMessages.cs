using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.EnergySeller;

[Serializable, NetSerializable]
public sealed partial class ChangesSpeedChargingForSellingEnergy : BoundUserInterfaceMessage
{
    public int? Now { get; set; }
    public int? Max { get; set; }
    public ChangesSpeedChargingForSellingEnergy(int? now = null, int? max = null)
    {
        Now = now;
        Max = max;
    }
}
public sealed partial class ChangesSellingForSellingEnergy : BoundUserInterfaceMessage
{
    public int? Now { get; set; }
    public int? Max { get; set; }
    public ChangesSellingForSellingEnergy(int? now = null, int? max = null)
    {
        Now = now;
        Max = max;
    }
}
