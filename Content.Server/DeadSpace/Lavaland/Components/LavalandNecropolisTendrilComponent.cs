using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class LavalandNecropolisTendrilComponent : Component
{
    [DataField]
    public List<EntProtoId> SpawnPrototypes = new()
    {
        "MobWatcherLavaland",
    };

    [DataField]
    public EntProtoId ChestPrototype = "LavalandNecropolisChest";

    [DataField]
    public EntProtoId CollapsePrototype = "LavalandNecropolisTendrilCollapse";

    [DataField]
    public int MaxActiveMobs = 3;

    [DataField]
    public TimeSpan SpawnInterval = TimeSpan.FromSeconds(30);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextSpawnTime = TimeSpan.Zero;

    [DataField]
    public float SpawnRadius = 1.5f;

    [DataField]
    public int ClearRadius = 1;

    public readonly HashSet<EntityUid> ActiveMobs = new();

    public bool Destroyed;
}
