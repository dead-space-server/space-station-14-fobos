using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.SponsorLoadout;

[Prototype]
public sealed partial class SponsorLoadoutPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("entity", required: true)]
    public EntProtoId EntityId { get; private set; } = default!;

    [DataField]
    public bool SponsorOnly = false;

    [DataField]
    public List<ProtoId<JobPrototype>>? WhitelistJobs { get; private set; }

    [DataField]
    public List<ProtoId<JobPrototype>>? BlacklistJobs { get; private set; }

    [DataField]
    public List<ProtoId<SpeciesPrototype>>? SpeciesRestrictions { get; private set; }
}
