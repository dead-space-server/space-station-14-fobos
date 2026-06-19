using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.AppHub;

[Prototype]
public sealed partial class AppCatalogEntryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public LocId Name = string.Empty;

    [DataField]
    public LocId Description = string.Empty;

    [DataField]
    public string Category = string.Empty;

    [DataField]
    public string ProgramId = string.Empty;
}
