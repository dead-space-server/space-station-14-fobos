using Content.Shared.Store;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server.Store.Conditions;

/// <summary>
/// Filters out an entry based on the components or tags on the store itself.
/// </summary>
public sealed partial class StoreWhitelistCondition : ListingCondition
{
    private static readonly ProtoId<TagPrototype> DebugUplinkTag = "DebugUplink"; // DS14

    /// <summary>
    /// A whitelist of tags or components.
    /// </summary>
    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// A blacklist of tags or components.
    /// </summary>
    [DataField("blacklist")]
    public EntityWhitelist? Blacklist;

    public override bool Condition(ListingConditionArgs args)
    {
        if (args.StoreEntity == null)
            return false;

        var ent = args.EntityManager;
        // DS14-start
        var tagSystem = ent.System<TagSystem>();
        if (tagSystem.HasTag(args.StoreEntity.Value, DebugUplinkTag))
            return true;
        // DS14-end

        var whitelistSystem = ent.System<EntityWhitelistSystem>();

        if (whitelistSystem.IsWhitelistFail(Whitelist, args.StoreEntity.Value) ||
            whitelistSystem.IsWhitelistPass(Blacklist, args.StoreEntity.Value))
            return false;

        return true;
    }
}
