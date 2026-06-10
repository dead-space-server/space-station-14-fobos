// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.DeadSpace.SpawnERTShuttleCommand;

/// <summary>
/// ERT shuttle id and path for loading it.
/// </summary>
[Prototype("ertShuttle")]
public sealed partial class ERTShuttlePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)] public ResPath Path = new("Maps/Shuttles/dart.yml");

    [DataField]
    public ProtoId<TagPrototype>? DockTag;
}
