// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Client.UserInterface;
using Content.Shared.DeadSpace.EnergySeller;

namespace Content.Client.DeadSpace.EnergySeller;

public sealed class EnergySellerBoundUserInterface : BoundUserInterface
{
    private EnergySellerUserInterface? _menu;
    public EnergySellerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);

    }
    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<EnergySellerUserInterface>();
        _menu.OnConfirmSpeedCharge += SendSpeedChage;
        _menu.OnConfirmSellLimit += SendMaxCharge;
    }
    private void SendSpeedChage(Dictionary<int, string> message)
    {
        ChangesForSellingEnergy cooking = new ChangesForSellingEnergy(true);

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
        ChangesForSellingEnergy cooking = new ChangesForSellingEnergy(false);
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
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        var castState = (EnergySellerBoundUserInterfaceState)state;
        _menu?.UpdateState(castState); //Update window state
    }
}
