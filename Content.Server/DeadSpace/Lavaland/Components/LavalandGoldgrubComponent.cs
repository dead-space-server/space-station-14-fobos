using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandGoldgrubComponent : Component
{
    [DataField]
    public int MaxStoredOre = 30;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5);

    [DataField]
    public TimeSpan BurrowDelay = TimeSpan.FromSeconds(10);

    [DataField]
    public float OreSearchRange = 8f;

    [DataField]
    public float EatRange = 0.9f;

    [DataField]
    public float ThreatRange = 5f;

    [DataField]
    public float FleeRange = 3.2f;

    [DataField]
    public float BurrowStartRange = 1.35f;

    [DataField]
    public float GreedyOreRange = 2.5f;

    [DataField]
    public float FleeTargetDistance = 6f;

    [DataField]
    public TimeSpan MinFleeTimeBeforeBurrow = TimeSpan.FromSeconds(2.5);

    [DataField]
    public TimeSpan WanderInterval = TimeSpan.FromSeconds(3);

    [DataField]
    public TimeSpan WanderRetryInterval = TimeSpan.FromSeconds(0.75);

    [DataField]
    public int WanderTargetAttempts = 10;

    [DataField]
    public float WanderDistanceMin = 2f;

    [DataField]
    public float WanderDistanceMax = 6f;

    [DataField]
    public List<ProtoId<StackPrototype>> InitialOre =
    [
        "SilverOre",
        "GoldOre",
        "UraniumOre",
        "DiamondOre"
    ];

    [DataField]
    public HashSet<ProtoId<StackPrototype>> EdibleOre =
    [
        "SilverOre",
        "GoldOre",
        "UraniumOre",
        "DiamondOre"
    ];

    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> StoredOre = new();

    [ViewVariables]
    public TimeSpan NextUpdate;

    [ViewVariables]
    public TimeSpan? BurrowAt;

    [ViewVariables]
    public TimeSpan? FleeStartedAt;

    [ViewVariables]
    public TimeSpan NextWanderAt;

    [ViewVariables]
    public bool DroppedOre;
}
