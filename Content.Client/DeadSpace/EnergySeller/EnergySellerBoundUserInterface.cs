using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

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
    }
}
