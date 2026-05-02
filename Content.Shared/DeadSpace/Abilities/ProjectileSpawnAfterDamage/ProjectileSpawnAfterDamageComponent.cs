using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Damage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ProjectileSpawnAfterDamageComponent : Component
{
    /// <summary>
    /// Прототип снаряда.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId? Entity = "MeteorSmall";

    /// <summary>
    /// Сколько снарядов вылетит.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Count = 3;

    /// <summary>
    /// Порог урона для активации.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Threshold = 25f;

    /// <summary>
    /// Скорость вылетающих снарядов.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ProjectileSpeed = 12f;

    [DataField, AutoNetworkedField]
    public float AccumulatedDamage = 0f; // накопленный урон между ударами

    [DataField, AutoNetworkedField]
    public float SpawnOffset = 2f; // смещение спавна, как в ShotInACircleSystem
}