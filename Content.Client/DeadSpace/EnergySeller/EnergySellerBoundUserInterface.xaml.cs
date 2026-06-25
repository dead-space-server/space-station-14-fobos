using Robust.Client.UserInterface.XAML;

namespace Content.Client.DeadSpace.EnergySeller;

public sealed class EnergySellerBoundUserInterface : BoundUserInterface
{
    public EnergySellerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        RobustXamlLoader.Load(this);
    }
}
