using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Ninja.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NinjaShurikenAbilityComponent : Component
{
    [DataField]
    public EntProtoId Action = "ToggleNinjaShurikenAbility";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// The item that will appear be spawned by the action.
    /// </summary>
    [DataField]
    public EntProtoId SpawnedPrototype = "WeaponPistolN1984";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionItemUid;

    /// <summary>
    /// The container ID used to store the item.
    /// </summary>
    public const string ContainerId = "ninja-shuriken-hand-container";
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NinjaShurikenAbilityItemComponent : Component
{
    /// <summary>
    /// The action that marked this item.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? SummoningAction;
}
public sealed partial class ShurikenHandRetractEvent : InstantActionEvent;
