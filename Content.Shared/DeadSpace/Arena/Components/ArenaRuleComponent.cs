using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.Arena;

[DataDefinition]
public sealed partial class ArenaWeaponLoadout
{
    [DataField(required: true)]
    public string Gear = string.Empty;

    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField]
    public string Description = string.Empty;

    [DataField(required: true)]
    public string Category = string.Empty;

    [DataField(required: true)]
    public string Sprite = string.Empty;
}

[RegisterComponent]
[NetworkedComponent]
public sealed partial class ArenaRuleComponent : Component
{
    [DataField]
    public EntityUid? ArenaMap;

    [DataField]
    public HashSet<NetEntity> Players = new();

    [DataField]
    public bool Active;

    [DataField]
    public string Gear = "ArenaBaseGear";

    [DataField]
    public List<ArenaWeaponLoadout> WeaponLoadouts = new();

    [DataField]
    public TimeSpan CleanupInterval = TimeSpan.FromSeconds(60);

    public TimeSpan NextCleanup;
}
