using System.Linq;
using System.Numerics;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Shuttles.Systems;
using Content.Server.UserInterface;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeadSpace.BSAConsole;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork.Systems;
using Content.Shared.Interaction;
using Content.Shared.Pinpointer;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.BSAConsole;

public sealed class BSAConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DeviceListSystem _deviceList = default!;
    [Dependency] private readonly ShuttleConsoleSystem _shuttleConsole = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    private const float RadarMaxRange = 512f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BSAConsoleComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<BSAConsoleComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<BSAConsoleComponent, AfterActivatableUIOpenEvent>(OnUiOpen);
        SubscribeLocalEvent<BSAConsoleComponent, BSAConsoleFireMessage>(OnFire);
        SubscribeLocalEvent<BSAConsoleComponent, BSAConsoleSwitchViewMessage>(OnSwitchView);
        SubscribeLocalEvent<BSAConsoleComponent, BSAConsoleSelectGridMessage>(OnSelectGrid);
        SubscribeLocalEvent<BSAConsoleComponent, BSAConsoleEjectDiskMessage>(OnEjectDisk);
        SubscribeLocalEvent<BSAConsoleComponent, EntInsertedIntoContainerMessage>(OnContainerInserted);
        SubscribeLocalEvent<BSAConsoleComponent, EntRemovedFromContainerMessage>(OnContainerRemoved);
        SubscribeLocalEvent<BSAConsoleComponent, DeviceListUpdateEvent>(OnDeviceListUpdated);
    }

    private void OnInit(EntityUid uid, BSAConsoleComponent comp, ComponentInit args)
    {
        _itemSlots.AddItemSlot(uid, BSAConsoleComponent.DiskSlotId, comp.DiskSlot);
    }

    private void OnRemove(EntityUid uid, BSAConsoleComponent comp, ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(uid, comp.DiskSlot);
    }

    private void OnUiOpen(EntityUid uid, BSAConsoleComponent comp, AfterActivatableUIOpenEvent args)
    {
        UpdateUiState(uid, comp);
    }

    private void OnFire(EntityUid uid, BSAConsoleComponent comp, BSAConsoleFireMessage msg)
    {
        if (comp.LinkedBSA == null || !TryComp<BluespaceArtilleryComponent>(comp.LinkedBSA.Value, out var bsa))
            return;

        if (!bsa.IsReady)
            return;

        if (_timing.CurTime.TotalSeconds < bsa.CooldownEnd)
            return;

        if (comp.HasPendingShot)
            return;

        // Target map comes from the client's MapCoordinates
        var targetMapId = new MapId(msg.MapId);

        if (targetMapId == MapId.Nullspace)
            return;

        // Store pending shot — explosion in PendingShotDelay seconds
        comp.HasPendingShot = true;
        comp.PendingShotTimeLeft = comp.PendingShotDelay;
        comp.PendingShotMapId = msg.MapId;

        // msg.X/Y are now world MapCoordinates from the client
        comp.PendingShotX = msg.X;
        comp.PendingShotY = msg.Y;

        // Store offset from selected grid center for tracking
        comp.PendingShotOffsetX = 0f;
        comp.PendingShotOffsetY = 0f;

        if (comp.CurrentViewMode == "Grid" && comp.SelectedGridUid != null)
        {
            var gridWorldPos = _transform.GetWorldPosition(comp.SelectedGridUid.Value);
            comp.PendingShotOffsetX = msg.X - gridWorldPos.X;
            comp.PendingShotOffsetY = msg.Y - gridWorldPos.Y;
        }

        // Lock the BSA so it can't fire again during the delay
        bsa.IsReady = false;
        Dirty(comp.LinkedBSA.Value, bsa);

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        comp.LogEntries.Add($"[{timestamp}] [ЗАЛП]: Залп инициирован. Цель: X: {msg.X:F1}, Y: {msg.Y:F1}. Взрыв через {comp.PendingShotDelay}с.");

        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void OnSwitchView(EntityUid uid, BSAConsoleComponent comp, BSAConsoleSwitchViewMessage msg)
    {
        comp.CurrentViewMode = msg.ViewMode;

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        comp.LogEntries.Add($"[{timestamp}] [НАВИГАЦИЯ]: Изменен режим визира -> {msg.ViewMode}.");

        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void OnSelectGrid(EntityUid uid, BSAConsoleComponent comp, BSAConsoleSelectGridMessage msg)
    {
        comp.SelectedGridName = msg.GridName;
        comp.SelectedGridUid = null;
        comp.CurrentViewMode = "Grid";

        // Find the grid entity
        comp.SelectedGridUid = FindGridEntity(uid, comp, msg.GridName);

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        comp.LogEntries.Add($"[{timestamp}] [НАВИГАЦИЯ]: Выбран грид: \"{msg.GridName}\".");

        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void OnEjectDisk(EntityUid uid, BSAConsoleComponent comp, BSAConsoleEjectDiskMessage msg)
    {
        _itemSlots.TryEject(uid, comp.DiskSlot, msg.Actor, out _);
        ClearDiskData(uid, comp);
    }

    private void OnContainerInserted(EntityUid uid, BSAConsoleComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != comp.DiskSlot.ID)
            return;

        if (!TryComp<ShuttleDestinationCoordinatesComponent>(args.Entity, out var diskCoords))
            return;

        if (diskCoords.Destination == null || !HasComp<MetaDataComponent>(diskCoords.Destination.Value))
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            comp.LogEntries.Add($"[{timestamp}] [ДИСК]: Диск не содержит валидных координат назначения.");
            Dirty(uid, comp);
            UpdateUiState(uid, comp);
            return;
        }

        var mapUid = diskCoords.Destination.Value;
        comp.TargetMapUid = mapUid;
        comp.TargetMapName = MetaData(mapUid).EntityName;
        comp.HasDisk = true;

        var ts = DateTime.Now.ToString("HH:mm:ss");
        var gridCount = GetGridNames(uid, comp).Count;
        comp.LogEntries.Add($"[{ts}] [ДИСК]: Считаны данные сектора: \"{comp.TargetMapName}\". Гридов на карте: {gridCount}.");

        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void OnContainerRemoved(EntityUid uid, BSAConsoleComponent comp, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != comp.DiskSlot.ID)
            return;

        ClearDiskData(uid, comp);
    }

    private void ClearDiskData(EntityUid uid, BSAConsoleComponent comp)
    {
        comp.TargetMapUid = null;
        comp.TargetMapName = null;
        comp.HasDisk = false;
        comp.SelectedGridName = null;

        if (comp.CurrentViewMode == "Grid" || comp.CurrentViewMode == "MassScannerDisk")
            comp.CurrentViewMode = "MassScannerLocal";

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        comp.LogEntries.Add($"[{timestamp}] [ДИСК]: Диск сектора извлечён.");

        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void OnDeviceListUpdated(EntityUid uid, BSAConsoleComponent comp, DeviceListUpdateEvent args)
    {
        TryFindBSA(uid, comp);
    }

    private void TryFindBSA(EntityUid uid, BSAConsoleComponent comp)
    {
        if (!TryComp<DeviceListComponent>(uid, out var deviceList))
            return;

        comp.LinkedBSA = null;

        foreach (var device in _deviceList.GetAllDevices(uid, deviceList))
        {
            if (TryComp<BluespaceArtilleryComponent>(device, out _))
            {
                comp.LinkedBSA = device;

                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                comp.LogEntries.Add($"[{timestamp}] [СЕТЬ]: Обнаружен сигнал БСА. Сопряжение успешно.");
                break;
            }
        }

        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BSAConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Pending shot countdown
            if (comp.HasPendingShot)
            {
                comp.PendingShotTimeLeft -= frameTime;

                if (comp.PendingShotTimeLeft <= 0f)
                {
                    FirePendingShot(uid, comp);
                }

                Dirty(uid, comp);
                UpdateUiState(uid, comp);
                continue;
            }

            if (comp.IsOnCooldown)
            {
                if (comp.LinkedBSA == null || !TryComp<BluespaceArtilleryComponent>(comp.LinkedBSA.Value, out var bsa))
                {
                    comp.IsOnCooldown = false;
                    comp.CooldownRemaining = 0;
                    Dirty(uid, comp);
                    UpdateUiState(uid, comp);
                    continue;
                }

                var remaining = (float)(bsa.CooldownEnd - _timing.CurTime.TotalSeconds);

                if (remaining <= 0)
                {
                    bsa.IsReady = true;
                    Dirty(comp.LinkedBSA.Value, bsa);

                    comp.IsOnCooldown = false;
                    comp.CooldownRemaining = 0;

                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    comp.LogEntries.Add($"[{timestamp}] [ЛОГ]: Охлаждение завершено. БЮ-конденсаторы заряжены. Орудие готово.");

                    Dirty(uid, comp);
                    UpdateUiState(uid, comp);

                    continue;
                }

                comp.CooldownRemaining = remaining;
                Dirty(uid, comp);

                if ((int)remaining != (int)(remaining + frameTime) && remaining > 0)
                {
                    UpdateUiState(uid, comp);
                }
            }
            else
            {
                // Always update UI state so radar tracks moving grids
                UpdateUiState(uid, comp);
            }
        }
    }

    private void FirePendingShot(EntityUid uid, BSAConsoleComponent comp)
    {
        if (comp.LinkedBSA == null || !TryComp<BluespaceArtilleryComponent>(comp.LinkedBSA.Value, out var bsa))
        {
            comp.HasPendingShot = false;
            return;
        }

        var targetMapId = new MapId(comp.PendingShotMapId);

        if (targetMapId == MapId.Nullspace)
        {
            comp.HasPendingShot = false;
            return;
        }

        // Recalculate position from grid's current center + stored offset
        var shotX = comp.PendingShotX;
        var shotY = comp.PendingShotY;

        if (comp.CurrentViewMode == "Grid" && comp.SelectedGridUid != null)
        {
            var gridWorldPos = _transform.GetWorldPosition(comp.SelectedGridUid.Value);
            shotX = gridWorldPos.X + comp.PendingShotOffsetX;
            shotY = gridWorldPos.Y + comp.PendingShotOffsetY;
        }

        var mapPos = new MapCoordinates(shotX, shotY, targetMapId);

        _explosion.QueueExplosion(
            mapPos,
            "Radioactive",
            2000f,
            5f,
            100f,
            comp.LinkedBSA.Value);

        comp.HasPendingShot = false;

        bsa.CooldownEnd = (float)(_timing.CurTime.TotalSeconds + bsa.CooldownDuration);
        Dirty(comp.LinkedBSA.Value, bsa);

        comp.IsOnCooldown = true;
        comp.CooldownRemaining = bsa.CooldownDuration;

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        comp.LogEntries.Add($"[{timestamp}] [ВЗРЫВ]: Достигнуты координаты X: {comp.PendingShotX:F1}, Y: {comp.PendingShotY:F1}.");
        comp.LogEntries.Add($"[{timestamp}] [ЛОГ]: Протокол охлаждения ствола активирован. КД: {(int)bsa.CooldownDuration} сек.");

        Dirty(uid, comp);
        UpdateUiState(uid, comp);
    }

    private void UpdateUiState(EntityUid uid, BSAConsoleComponent comp)
    {
        if (!_ui.HasUi(uid, BSAConsoleUiKey.Key))
            return;

        var isConnected = comp.LinkedBSA != null;
        string? bsaName = null;
        var cooldownDuration = 60f;

        if (isConnected && TryComp<BluespaceArtilleryComponent>(comp.LinkedBSA!.Value, out var bsa))
        {
            bsaName = Comp<MetaDataComponent>(comp.LinkedBSA!.Value).EntityName;
            cooldownDuration = bsa.CooldownDuration;
        }

        var localRadarState = BuildLocalRadarState(uid);
        var diskRadarState = BuildDiskRadarState(comp);
        var allGrids = BuildUnifiedGridList(uid, comp);
        var gridRadarState = BuildGridRadarState(uid, comp);

        var state = new BSAConsoleUiState(
            isConnected,
            bsaName,
            comp.IsOnCooldown,
            comp.CooldownRemaining,
            cooldownDuration,
            comp.CurrentViewMode,
            comp.LogEntries,
            comp.HasDisk,
            comp.TargetMapName,
            localRadarState,
            diskRadarState,
            allGrids,
            comp.SelectedGridName,
            comp.SelectedGridUid != null ? GetNetEntity(comp.SelectedGridUid.Value) : null,
            comp.HasPendingShot,
            comp.PendingShotTimeLeft,
            comp.PendingShotDelay,
            gridRadarState);

        _ui.SetUiState(uid, BSAConsoleUiKey.Key, state);
    }

    private NavInterfaceState BuildLocalRadarState(EntityUid uid)
    {
        var docks = _shuttleConsole.GetAllDocks();
        var consoleXform = Transform(uid);
        return new NavInterfaceState(RadarMaxRange, GetNetCoordinates(consoleXform.Coordinates), consoleXform.LocalRotation, docks);
    }

    private NavInterfaceState? BuildGridRadarState(EntityUid uid, BSAConsoleComponent comp)
    {
        if (comp.CurrentViewMode != "Grid" || comp.SelectedGridUid == null)
            return null;

        if (TryComp<NavMapComponent>(comp.SelectedGridUid.Value, out _))
            return null;

        var gridXform = Transform(comp.SelectedGridUid.Value);
        var docks = _shuttleConsole.GetAllDocks();
        return new NavInterfaceState(RadarMaxRange, GetNetCoordinates(gridXform.Coordinates), Angle.Zero, docks);
    }

    private NavInterfaceState? BuildDiskRadarState(BSAConsoleComponent comp)
    {
        if (comp.TargetMapUid == null)
            return null;

        var docks = _shuttleConsole.GetAllDocks();
        var targetMapId = ResolveTargetMapId(comp.TargetMapUid.Value);

        if (targetMapId == MapId.Nullspace)
            return null;

        foreach (var grid in _mapManager.GetAllGrids(targetMapId))
        {
            var gridXform = Transform(grid.Owner);
            return new NavInterfaceState(RadarMaxRange, GetNetCoordinates(gridXform.Coordinates), Angle.Zero, docks);
        }

        var targetXform = Transform(comp.TargetMapUid.Value);
        return new NavInterfaceState(RadarMaxRange, GetNetCoordinates(targetXform.Coordinates), Angle.Zero, docks);
    }

    private List<BSAGridEntry> BuildUnifiedGridList(EntityUid uid, BSAConsoleComponent comp)
    {
        var result = new List<BSAGridEntry>();
        CollectGridEntries(uid, comp, result);
        return result;
    }

    private void CollectGridEntries(EntityUid uid, BSAConsoleComponent comp, List<BSAGridEntry> result)
    {
        // Local grids
        var localMapId = Transform(uid).MapID;
        if (localMapId != MapId.Nullspace)
        {
            foreach (var grid in _mapManager.GetAllGrids(localMapId))
            {
                var name = MetaData(grid.Owner).EntityName;
                if (!string.IsNullOrEmpty(name))
                    result.Add(new BSAGridEntry(name, "local"));
            }
        }

        // Disk grids
        if (comp.TargetMapUid != null)
        {
            var diskMapId = ResolveTargetMapId(comp.TargetMapUid.Value);
            if (diskMapId != MapId.Nullspace && diskMapId != localMapId)
            {
                foreach (var grid in _mapManager.GetAllGrids(diskMapId))
                {
                    var name = MetaData(grid.Owner).EntityName;
                    if (!string.IsNullOrEmpty(name))
                        result.Add(new BSAGridEntry(name, "disk"));
                }
            }
        }
    }

    private List<string> GetGridNames(EntityUid uid, BSAConsoleComponent comp)
    {
        var entries = new List<BSAGridEntry>();
        CollectGridEntries(uid, comp, entries);
        return entries.Select(e => e.Name).ToList();
    }

    private MapId FindGridMapId(EntityUid uid, BSAConsoleComponent comp, string gridName)
    {
        var localMapId = Transform(uid).MapID;
        if (localMapId != MapId.Nullspace)
        {
            foreach (var grid in _mapManager.GetAllGrids(localMapId))
            {
                if (MetaData(grid.Owner).EntityName.Equals(gridName, StringComparison.OrdinalIgnoreCase))
                    return localMapId;
            }
        }

        if (comp.TargetMapUid != null)
        {
            var diskMapId = ResolveTargetMapId(comp.TargetMapUid.Value);
            if (diskMapId != MapId.Nullspace && diskMapId != localMapId)
            {
                foreach (var grid in _mapManager.GetAllGrids(diskMapId))
                {
                    if (MetaData(grid.Owner).EntityName.Equals(gridName, StringComparison.OrdinalIgnoreCase))
                        return diskMapId;
                }
            }
        }

        return Transform(uid).MapID;
    }

    /// <summary>
    /// Resolve a map entity UID to its MapId, trying multiple approaches.
    /// </summary>
    private MapId ResolveTargetMapId(EntityUid mapUid)
    {
        if (TryComp<MapComponent>(mapUid, out var mapComp))
            return mapComp.MapId;

        var xform = Transform(mapUid);
        if (xform.MapID != MapId.Nullspace)
            return xform.MapID;

        var query = EntityQueryEnumerator<MapComponent>();
        while (query.MoveNext(out var ent, out var mc))
        {
            if (ent == mapUid)
                return mc.MapId;
        }

        return MapId.Nullspace;
    }

    private EntityUid? FindGridEntity(EntityUid uid, BSAConsoleComponent comp, string gridName)
    {
        var localMapId = Transform(uid).MapID;
        if (localMapId != MapId.Nullspace)
        {
            foreach (var grid in _mapManager.GetAllGrids(localMapId))
            {
                if (MetaData(grid.Owner).EntityName.Equals(gridName, StringComparison.OrdinalIgnoreCase))
                    return grid.Owner;
            }
        }

        if (comp.TargetMapUid != null)
        {
            var diskMapId = ResolveTargetMapId(comp.TargetMapUid.Value);
            if (diskMapId != MapId.Nullspace && diskMapId != localMapId)
            {
                foreach (var grid in _mapManager.GetAllGrids(diskMapId))
                {
                    if (MetaData(grid.Owner).EntityName.Equals(gridName, StringComparison.OrdinalIgnoreCase))
                        return grid.Owner;
                }
            }
        }

        return null;
    }
}
