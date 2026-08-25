using Content.Shared.Shuttles.BUIStates;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.BSAConsole;

[Serializable, NetSerializable]
public sealed class BSAConsoleUiState : BoundUserInterfaceState
{
    public bool IsConnected;
    public string? BSAName;
    public bool IsOnCooldown;
    public float CooldownRemaining;
    public float CooldownDuration;
    public string CurrentViewMode;
    public List<string> LogEntries;

    /// <summary>
    /// True if a valid CoordinatesDisk is inserted.
    /// </summary>
    public bool HasDisk;

    /// <summary>
    /// Name of the target map from the disk.
    /// </summary>
    public string? TargetMapName;

    /// <summary>
    /// Names of grids found on the target map.
    /// </summary>
    public List<string> AvailableGrids;

    /// <summary>
    /// Radar state for mass scanner mode. Null if not available.
    /// </summary>
    public NavInterfaceState? RadarState;

    /// <summary>
    /// NetEntity of the selected grid for map view.
    /// </summary>
    public NetEntity? SelectedGridUid;

    /// <summary>
    /// Human-readable name of the selected grid.
    /// </summary>
    public string? SelectedGridName;

    public BSAConsoleUiState(
        bool isConnected,
        string? bsaName,
        bool isOnCooldown,
        float cooldownRemaining,
        float cooldownDuration,
        string currentViewMode,
        List<string> logEntries,
        bool hasDisk,
        string? targetMapName,
        List<string> availableGrids,
        NavInterfaceState? radarState = null,
        NetEntity? selectedGridUid = null,
        string? selectedGridName = null)
    {
        IsConnected = isConnected;
        BSAName = bsaName;
        IsOnCooldown = isOnCooldown;
        CooldownRemaining = cooldownRemaining;
        CooldownDuration = cooldownDuration;
        CurrentViewMode = currentViewMode;
        LogEntries = logEntries;
        HasDisk = hasDisk;
        TargetMapName = targetMapName;
        AvailableGrids = availableGrids;
        RadarState = radarState;
        SelectedGridUid = selectedGridUid;
        SelectedGridName = selectedGridName;
    }
}
