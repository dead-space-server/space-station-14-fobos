// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Damage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ProjectileSpawnAfterDamageComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId? Entity = "MeteorSmall";

    [DataField, AutoNetworkedField]
    public int Count = 3;

    [DataField, AutoNetworkedField]
    public float Threshold = 25f;

    [DataField, AutoNetworkedField]
    public float ProjectileSpeed = 12f;

    [DataField, AutoNetworkedField]
    public float AccumulatedDamage = 0f;

    [DataField, AutoNetworkedField]
    public float SpawnOffset = 2f;
}