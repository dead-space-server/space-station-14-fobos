// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Nuke;

[Prototype]
public sealed partial class NukeCodeSendReasonPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name { get; private set; }

    [DataField(required: true)]
    public LocId Announcement { get; private set; }
}

public static class NukeCodeSendReasonIds
{
    public static readonly ProtoId<NukeCodeSendReasonPrototype> Manual = "Manual";
    public static readonly ProtoId<NukeCodeSendReasonPrototype> BlobCriticalMass = "BlobCriticalMass";
    public static readonly ProtoId<NukeCodeSendReasonPrototype> SpiderTerrorCritical = "SpiderTerrorCritical";
}
