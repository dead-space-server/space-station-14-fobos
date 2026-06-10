// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.Shuttles.Components;

[DataDefinition]
public sealed partial class RadarBlipTagEntry
{
    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag;

    [DataField]
    public Color Color = Color.White;
}