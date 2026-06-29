using Robust.Shared.GameStates;
using Content.Shared.Cargo.Prototypes;

namespace Content.Shared.DeadSpace.EnergySeller;

[RegisterComponent, NetworkedComponent]
public sealed partial class EnergySellerComponent : Component
{
    /// <summary>
    /// Коэффицент надбавки за продаваемое количество электроэнергии.
    /// По умолчанию стоит надбавка за каждый мегавват, то есть за 1 продаваемый мегават множитель будет 2
    /// </summary>
    [DataField]
    public int AdditionalCoefficient = 1000000;
    [DataField]
    public string Account = "Engineering";
    [DataField]
    public int MaxChargeRate = 1000000;
    [DataField]
    public int MaxLimit = 150000;
}
