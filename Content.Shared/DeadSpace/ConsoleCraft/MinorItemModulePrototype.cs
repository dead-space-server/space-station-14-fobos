// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.Markdown.Mapping;

namespace Content.Shared.DeadSpace.ConsoleCraft;

[Prototype]
public sealed partial class MinorItemModulePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("moduleItem")]
    public EntProtoId? ModuleItem;

    [DataField("tag")]
    public List<string>? Tag;

    [DataField("description")]
    public string Description = string.Empty;

    [DataField("ifModSuit")]
    public EntProtoId? IfModSuit;

    [DataField("components", customTypeSerializer: typeof(ComponentDataSerializer))]
    public Dictionary<string, MappingDataNode> Components = new();
}