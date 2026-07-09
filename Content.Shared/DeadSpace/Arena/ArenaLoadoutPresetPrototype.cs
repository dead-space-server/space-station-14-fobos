using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Arena;

[Prototype]
public sealed partial class ArenaLoadoutPresetPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string NameLoc = string.Empty;

    [DataField]
    public string DescLoc = string.Empty;

    [DataField]
    public string IconPrototype = string.Empty;

    [DataField]
    public string Category = string.Empty;

    /// <summary>
    /// Slot → entity prototype ID. E.g. outerClothing: ClothingOuterArmorBasic.
    /// </summary>
    [DataField]
    public Dictionary<string, string> Equipment = new();

    /// <summary>
    /// Items placed in hand on spawn.
    /// </summary>
    [DataField]
    public List<string> Inhand = new();

    /// <summary>
    /// Slot → list of items to put inside its storage.
    /// </summary>
    [DataField]
    public Dictionary<string, List<string>> Storage = new();
}
