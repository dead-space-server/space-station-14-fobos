// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Dataset;
using Content.Shared.FixedPoint;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Random;
using Content.Shared.Roles.Components;
using Content.Shared.Store;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.DeadSpace.Traitor;

[RegisterComponent, Access(typeof(TraitorUltraRuleSystem))]
public sealed partial class TraitorUltraRuleComponent : Component
{
    public readonly Dictionary<EntityUid, TraitorUltraMindState> Minds = new();
    public readonly Dictionary<EntityUid, string?> PendingRecruitOffers = new();

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextCheck;

    [DataField]
    public TimeSpan CheckDelay = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan UpgradeOfferDelay = TimeSpan.FromSeconds(12);

    [DataField]
    public TimeSpan BountyPreparationTime = TimeSpan.FromMinutes(3);

    [DataField]
    public TimeSpan RewardDelay = TimeSpan.FromSeconds(10);

    [DataField]
    public FixedPoint2 UpgradeTelecrystals = 10;

    [DataField]
    public FixedPoint2 TraitorKillRewardTelecrystals = 8;

    [DataField]
    public int SecurityKillRewardCredits = 10000;

    [DataField]
    public int CaptainKillRewardCredits = 10000;

    [DataField]
    public ProtoId<CargoAccountPrototype> SecurityRewardAccount = "Security";

    [DataField]
    public FixedPoint2 RecruitTelecrystals = 10;

    [DataField]
    public ProtoId<LocalizedDatasetPrototype> CorporationDataset = "TraitorCorporations";

    [DataField]
    public EntProtoId<MindRoleComponent> TraitorMindRole = "MindRoleTraitor";

    [DataField]
    public EntProtoId UltraMindRole = "MindRoleTraitorUltra";

    [DataField]
    public EntProtoId RecruitMindRole = "MindRoleTraitor";

    [DataField]
    public EntProtoId CommandKillObjective = "KillRandomHeadObjective";

    [DataField]
    public ProtoId<WeightedRandomPrototype> BaseObjectiveGroups = "TraitorUltraObjectiveGroups";

    [DataField]
    public float BaseObjectiveMaxDifficulty = 5f;

    [DataField]
    public int BaseObjectiveMaxPicks = 20;

    [DataField]
    public ProtoId<WeightedRandomPrototype> RecruitObjectiveGroups = "TraitorObjectiveGroups";

    [DataField]
    public float RecruitObjectiveMaxDifficulty = 5f;

    [DataField]
    public int RecruitObjectiveMaxPicks = 20;

    [DataField]
    public List<EntProtoId> HighRiskStealObjectives = new()
    {
        "NukeDiskStealObjective",
        "CaptainIDStealObjective",
        "CaptainGunStealObjective",
        "CaptainJetpackStealObjective",
        "HandTeleporterStealObjective",
        "RDHardsuitStealObjective",
        "WeaponX01StealObjective",
        "PistolBlueShieldStealObjective",
        "TabletRDStealObjective",
        "PinpointerNuclearStealObjective",
    };

    [DataField]
    public List<EntProtoId> PostUpgradeObjectives = new()
    {
        "TraitorUltraDestroyAtmosGasMinersObjective",
        "TraitorUltraDestroyAmeControllerObjective",
        "TraitorUltraHijackShuttleObjective",
        "TraitorUltraDestroyServersObjective",
    };

    [DataField]
    public List<EntProtoId> RarePostUpgradeObjectives = new()
    {
        "TraitorUltraKillHalfSecurityObjective",
    };

    [DataField]
    public EntProtoId PostUpgradeSurviveObjective = "TraitorUltraSurviveObjective";

    [DataField]
    public float RarePostUpgradeObjectiveProbability = 0.15f;

    [DataField]
    public HashSet<EntProtoId> UpgradeCompletionIgnoredObjectives = new()
    {
        "EscapeShuttleObjective",
        "DieObjective",
    };

    [DataField]
    public SoundSpecifier UpgradeSound = new SoundPathSpecifier("/Audio/_DeadSpace/TraitorUltra/ultra_role_assigned.ogg");

    [DataField]
    public SoundSpecifier BountyAnnouncementSound = new SoundPathSpecifier("/Audio/_DeadSpace/TraitorUltra/contract_transfer_announcement.ogg");

    [DataField]
    public ProtoId<CurrencyPrototype> TelecrystalCurrency = "Telecrystal";
}

public sealed class TraitorUltraMindState
{
    public TraitorUltraStage Stage = TraitorUltraStage.Initial;
    public List<EntityUid> InitialObjectives = new();
    public string? OriginalCorporation;
    public string? NewCorporation;
    public string? AgentName;
    public FixedPoint2 BountyReward;
    public TimeSpan NextEventTime;
    public bool BountyAnnounced;
    public bool BountyResolved;
    public EntityUid? BountyBody;
}

public enum TraitorUltraStage : byte
{
    Initial,
    CompletionPopupSent,
    OfferOpen,
    Declined,
    Upgraded,
    BountyAnnounced,
    Resolved,
}
