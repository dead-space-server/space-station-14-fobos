using Robust.Shared.Prototypes;

namespace Content.Shared.Nuke;

[Prototype("nukeCodeSendReason")]
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
