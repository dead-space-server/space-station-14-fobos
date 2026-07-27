// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.EnergySeller;

[Serializable, NetSerializable]
public sealed partial class ChangesForSellingEnergy : BoundUserInterfaceMessage
{
    public int? Now { get; set; }
    public int? Max { get; set; }
    public bool SpeedOrLimit { get; set; }
    public ChangesForSellingEnergy(bool speedOrLimit, int? now = null, int? max = null)
    {
        Now = now;
        Max = max;
        SpeedOrLimit = speedOrLimit;
    }
}

[Serializable, NetSerializable]
public sealed class EnergySellerBoundUserInterfaceState : BoundUserInterfaceState
{
    public int MaxChargeRate;
    public int MaxLimit;
    public int NowChargeRate;
    public int NowLimit;

    public EnergySellerBoundUserInterfaceState(int maxChargeRate, int maxLimit, int nowChargeRate, int nowLimit)
    {
        MaxChargeRate = maxChargeRate;
        MaxLimit = maxLimit;
        NowChargeRate = nowChargeRate;
        NowLimit = nowLimit;
    }
}
[Serializable, NetSerializable]
public enum ESBControllerUiKey
{
    Key
}
