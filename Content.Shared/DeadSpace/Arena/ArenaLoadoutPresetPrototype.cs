using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Arena;

[Prototype]
public sealed partial class ArenaLoadoutPresetPrototype : IPrototype, IEquipmentLoadout
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

    [DataField]
    public Dictionary<string, EntProtoId> Equipment { get; set; } = new();

    [DataField]
    public List<EntProtoId> Inhand { get; set; } = new();

    [DataField]
    public Dictionary<string, List<EntProtoId>> Storage { get; set; } = new();
}
