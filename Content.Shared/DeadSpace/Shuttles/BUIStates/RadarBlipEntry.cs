// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.Shuttles.Components;

[DataDefinition]
public sealed partial class RadarBlipEntry
{

    [DataField(required: true)]
    public string Component = string.Empty;

    [DataField]
    public Color Color = Color.White;
}