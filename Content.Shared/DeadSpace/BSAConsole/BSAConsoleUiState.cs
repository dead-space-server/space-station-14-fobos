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

    public bool HasDisk;
    public string? TargetMapName;

    // Local radar
    public NavInterfaceState? LocalRadarState;

    // Disk radar
    public NavInterfaceState? DiskRadarState;

    // Unified grid list — grids from local map + disk map
    public List<BSAGridEntry> AllGrids;

    // Currently selected grid
    public string? SelectedGridName;
    public NetEntity? SelectedGridUid;

    // Pending shot
    public bool HasPendingShot;
    public float PendingShotTimeLeft;
    public float PendingShotDelay;

    // Grid radar state (for grids without NavMapComponent)
    public NavInterfaceState? GridRadarState;

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
        NavInterfaceState? localRadarState,
        NavInterfaceState? diskRadarState,
        List<BSAGridEntry> allGrids,
        string? selectedGridName,
        NetEntity? selectedGridUid,
        bool hasPendingShot,
        float pendingShotTimeLeft,
        float pendingShotDelay,
        NavInterfaceState? gridRadarState)
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
        LocalRadarState = localRadarState;
        DiskRadarState = diskRadarState;
        AllGrids = allGrids;
        SelectedGridName = selectedGridName;
        SelectedGridUid = selectedGridUid;
        HasPendingShot = hasPendingShot;
        PendingShotTimeLeft = pendingShotTimeLeft;
        PendingShotDelay = pendingShotDelay;
        GridRadarState = gridRadarState;
    }
}

[Serializable, NetSerializable]
public sealed class BSAGridEntry
{
    public string Name { get; }
    public string Source { get; }

    public BSAGridEntry(string name, string source)
    {
        Name = name;
        Source = source;
    }
}
