using Content.Shared.Storage;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Lavaland.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LavalandMiningVoucherComponent : Component
{
    [DataField(required: true)]
    public List<LavalandMiningVoucherReward> Rewards = new();
}

[DataDefinition]
public sealed partial class LavalandMiningVoucherReward
{
    [DataField(required: true)]
    public LocId Name = string.Empty;

    [DataField(required: true)]
    public List<EntitySpawnEntry> Entries = new();
}
