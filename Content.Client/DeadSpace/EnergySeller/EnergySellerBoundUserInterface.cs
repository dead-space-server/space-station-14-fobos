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
        _menu.OnConfirmSpeedCharge += SendSpeedCharge;
        _menu.OnConfirmSellLimit += SendMaxCharge;
    }

    private void SendSpeedCharge(int value, int value2)
    {
        SendMessage(new ChangesForSellingEnergy(true, value, value2));
    }

    private void SendMaxCharge(int value, int value2)
    {
        SendMessage(new ChangesForSellingEnergy(false, value, value2));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        var castState = (EnergySellerBoundUserInterfaceState)state;
        _menu?.UpdateState(castState);
    }
}
