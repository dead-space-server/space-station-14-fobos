namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandMiningToolComponent : Component
{
    [DataField]
    public int YieldMultiplier = 1;

    [DataField]
    public int CleaveTargets;

    [DataField]
    public int CleaveYieldMultiplier = 1;
}
