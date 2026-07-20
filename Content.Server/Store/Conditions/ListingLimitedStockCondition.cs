using Content.Shared.Store;

namespace Content.Server.Store.Conditions;

/// <summary>
/// Only allows a listing to be purchased a certain amount of times.
/// </summary>
public sealed partial class ListingLimitedStockCondition : ListingCondition
{
    /// <summary>
    /// The amount of times this listing can be purchased.
    /// </summary>
    [DataField("stock", required: true)]
    public int Stock;

    public override bool Condition(ListingConditionArgs args)
    {
        // DS14-start - expose the remaining purchase count to the store UI.
        var remainingStock = Math.Max(Stock - args.Listing.PurchaseAmount, 0);
        args.Listing.RemainingStock = remainingStock;
        return remainingStock > 0;
        // DS14-end
    }
}
