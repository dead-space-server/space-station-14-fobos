using Content.Shared.Access;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Store;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Lavaland.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LavalandMiningShopComponent : Component
{
    public const string CardSlotId = "LavalandMiningShop-card";
    public const string VoucherSlotId = "LavalandMiningShop-voucher";

    [DataField]
    public ItemSlot CardSlot = new();

    [DataField]
    public ItemSlot VoucherSlot = new();

    [DataField]
    public ProtoId<CurrencyPrototype> Currency = "LavalandMiningPoint";

    [DataField]
    public ProtoId<StoreCategoryPrototype> EmagCategory = "LavalandMiningContraband";

    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>> MiningAccess = new()
    {
        "Salvage",
        "SeniorSalvage",
    };
}
