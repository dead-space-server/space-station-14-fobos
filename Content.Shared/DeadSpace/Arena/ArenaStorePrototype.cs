using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Arena;

/// <summary>
/// Конфигурация магазина арены (вкладка «Закуп ТДМ»).
/// </summary>
[Prototype]
public sealed partial class ArenaStorePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>ID листингов аплинка, которые нельзя купить в магазине арены.</summary>
    [DataField]
    public List<ProtoId<ListingPrototype>> ExcludedListings = new();
}
