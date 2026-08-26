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

    // View mode: "MassScanner", "Grid"
    [DataField, AutoNetworkedField]
    public string CurrentViewMode = "MassScannerLocal";

    [DataField, AutoNetworkedField]
    public List<string> LogEntries = new();

    // Disk data
    [DataField, AutoNetworkedField]
    public EntityUid? TargetMapUid;

    [DataField, AutoNetworkedField]
    public string? TargetMapName;

    [DataField, AutoNetworkedField]
    public bool HasDisk;

    // Selected grid (for Grid mode)
    [DataField, AutoNetworkedField]
    public string? SelectedGridName;

    [DataField, AutoNetworkedField]
    public EntityUid? SelectedGridUid;

    // Pending shot (10s delay before explosion)
    [DataField, AutoNetworkedField]
    public bool HasPendingShot;

    [DataField, AutoNetworkedField]
    public float PendingShotX;

    [DataField, AutoNetworkedField]
    public float PendingShotY;

    [DataField, AutoNetworkedField]
    public float PendingShotTimeLeft;

    [DataField]
    public float PendingShotDelay = 10f;

    // Offset from selected grid center at fire time (for tracking moving grids)
    [DataField]
    public float PendingShotOffsetX;

    [DataField]
    public float PendingShotOffsetY;
}
