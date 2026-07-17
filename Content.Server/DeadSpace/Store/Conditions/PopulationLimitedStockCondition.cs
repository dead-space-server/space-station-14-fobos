// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Store;
using Robust.Server.Player;
using Robust.Shared.IoC;

namespace Content.Server.Store.Conditions;

/// <summary>
/// Limits listing stock, with a second limit used when the connected population is above a threshold.
/// </summary>
public sealed partial class PopulationLimitedStockCondition : ListingCondition
{
    [DataField(required: true)]
    public int Stock;

    [DataField(required: true)]
    public int HighPopulationStock;

    [DataField(required: true)]
    public int HighPopulationThreshold;

    public override bool Condition(ListingConditionArgs args)
    {
        var players = IoCManager.Resolve<IPlayerManager>();
        var availableStock = players.PlayerCount > HighPopulationThreshold
            ? HighPopulationStock
            : Stock;
        args.Listing.RemainingStock = Math.Max(availableStock - args.Listing.PurchaseAmount, 0);
        return args.Listing.RemainingStock > 0;
    }
}
