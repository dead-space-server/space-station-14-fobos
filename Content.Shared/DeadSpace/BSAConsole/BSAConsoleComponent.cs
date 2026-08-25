using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.BSAConsole;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BSAConsoleComponent : Component
{
    public const string DiskSlotId = "BSAConsole-DiskSlot";

    [DataField]
    public ItemSlot DiskSlot = new();

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedBSA;

    [DataField, AutoNetworkedField]
    public bool IsOnCooldown;

    [DataField, AutoNetworkedField]
    public float CooldownRemaining;

    [DataField, AutoNetworkedField]
    public string CurrentViewMode = "MassScanner";

    [DataField, AutoNetworkedField]
    public List<string> LogEntries = new();

    /// <summary>
    /// The map entity pointed to by the inserted CoordinatesDisk.
    /// When set, the console shows grids from this map.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? TargetMapUid;

    /// <summary>
    /// Human-readable name of the target map.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? TargetMapName;

    /// <summary>
    /// Names of all grids found on the target map.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> AvailableGrids = new();

    /// <summary>
    /// The grid entity currently selected for map view.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? SelectedGridUid;

    /// <summary>
    /// Human-readable name of the selected grid.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? SelectedGridName;

    /// <summary>
    /// Whether a valid coordinates disk is inserted.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool HasDisk;
}
