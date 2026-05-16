using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandLegionComponent : Component
{
    [DataField]
    public int MaxActiveHeads = 2;

    public readonly HashSet<EntityUid> ActiveHeads = new();
}

[RegisterComponent]
public sealed partial class LavalandLegionHeadComponent : Component
{
    [DataField]
    public EntProtoId InfestPrototype = "MobLavalandLegion";

    [DataField]
    public float InfestRange = 1.1f;

    [DataField]
    public TimeSpan InfestCheckInterval = TimeSpan.FromSeconds(0.5);

    public TimeSpan NextInfestCheck;
    public EntityUid? Source;
}

[RegisterComponent]
public sealed partial class LavalandLegionInfestedComponent : Component
{
}
