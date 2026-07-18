// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
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

    public Dictionary<ProtoId<CargoAccountPrototype>, double> Distribution = new();
}
