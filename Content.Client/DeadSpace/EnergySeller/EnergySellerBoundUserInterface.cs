using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Content.Shared.DeadSpace.EnergySeller;
using Robust.Shared.GameObjects;
using Content.Shared.Power.Components;

namespace Content.Client.DeadSpace.EnergySeller;

public sealed class EnergySellerBoundUserInterface : BoundUserInterface
{
    private EnergySellerUserInterface? _menu;
    [Dependency] private readonly IEntityManager _EntMan = default!;
    public EnergySellerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }
    protected override void Open()
    {
        base.Open();
        var getCompSeller = _EntMan.GetComponent<EnergySellerComponent>(Owner);
        var getCompBattery = _EntMan.GetComponent<BatteryComponent>(Owner);
        _menu = this.CreateWindow<EnergySellerUserInterface>();
        _menu.SetMaxSlider(getCompSeller.MaxChargeRate, getCompSeller.MaxLimit);
        _menu.SetBattarycomp((int)getCompBattery.ChargeRate, (int)getCompBattery.MaxCharge);
        _menu.OnConfirmSpeedCharge += SendSpeedChage;
        _menu.OnConfirmSellLimit += SendMaxCharge;
    }
    private void SendSpeedChage(Dictionary<int, string> message)
    {
        ChangesSpeedChargingForSellingEnergy cooking = new ChangesSpeedChargingForSellingEnergy();
        if (int.TryParse(message[1], out int intNow))
        {
            cooking.Now = intNow;
        }
        if (int.TryParse(message[2], out int intNowSecond))
        {
            cooking.Max = intNowSecond;
        }
        SendMessage(cooking);
    }
    private void SendMaxCharge(Dictionary<int, string> message)
    {
        ChangesSellingForSellingEnergy cooking = new ChangesSellingForSellingEnergy();
        if (int.TryParse(message[1], out int intNow))
        {
            cooking.Now = intNow;
        }
        if (int.TryParse(message[2], out int intNowSecond))
        {
            cooking.Max = intNowSecond;
        }
        SendMessage(cooking);
    }
}
