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

        if (bsa.IsReady == false)
            return;

        if (_timing.CurTime.TotalSeconds < bsa.CooldownEnd)
            return;

        if (comp.TargetMapUid == null)
            return;

        var targetXform = Transform(comp.TargetMapUid.Value);
        var mapPos = new MapCoordinates(msg.X, msg.Y, targetXform.MapID);

        _explosion.QueueExplosion(
            mapPos,
            "Default",
            250f,
            3f,
            50f,
            comp.LinkedBSA.Value);

        bsa.IsReady = false;
        bsa.CooldownEnd = (float)(_timing.CurTime.TotalSeconds + bsa.CooldownDuration);
        Dirty(comp.LinkedBSA.Value, bsa);

        comp.IsOnCooldown = true;
        comp.CooldownRemaining = bsa.CooldownDuration;

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        comp.LogEntries.Add($"[{timestamp}] [ЗАЛП]: Произведен выстрел по координатам X: {msg.X:F1}, Y: {msg.Y:F1}.");
        comp.LogEntries.Add($"[{timestamp}] [ЛОГ]: Активирован протокол охлаждения ствола БСА. КД: {(int)bsa.CooldownDuration} сек.");

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
        comp.SelectedGridUid = null;
        comp.SelectedGridName = null;

        if (comp.TargetMapUid == null)
            return;

        var targetMapXform = Transform(comp.TargetMapUid.Value);
        var mapId = targetMapXform.MapID;

        if (mapId == MapId.Nullspace)
            return;

        var grids = new List<Entity<MapGridComponent>>();
        _mapManager.FindGridsIntersecting(mapId, new Box2(-1e10f, -1e10f, 1e10f, 1e10f), ref grids, includeMap: false);

        foreach (var grid in grids)
        {
            if (!MetaData(grid.Owner).EntityName.Equals(msg.GridName, StringComparison.OrdinalIgnoreCase))
                continue;

            comp.SelectedGridUid = grid.Owner;
            comp.SelectedGridName = MetaData(grid.Owner).EntityName;
            break;
        }

        if (comp.SelectedGridUid == null)
        {
            comp.SelectedGridName = msg.GridName;
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            comp.LogEntries.Add($"[{timestamp}] [ОШИБКА]: Грид \"{msg.GridName}\" не найден на целевой карте.");
        }
        else
        {
            var ts = DateTime.Now.ToString("HH:mm:ss");
            comp.LogEntries.Add($"[{ts}] [НАВИГАЦИЯ]: Выбран грид: \"{comp.SelectedGridName}\".");
        }

        comp.CurrentViewMode = "Grid";

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

        if (diskCoords.Destination == null || !TryComp<MetaDataComponent>(diskCoords.Destination.Value, out var mapMeta))
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            comp.LogEntries.Add($"[{timestamp}] [ДИСК]: Диск не содержит валидных координат назначения.");
            Dirty(uid, comp);
            UpdateUiState(uid, comp);
            return;
        }

        var mapUid = diskCoords.Destination.Value;
        comp.TargetMapUid = mapUid;
        comp.TargetMapName = mapMeta.EntityName;
        comp.HasDisk = true;

        // Find all grids on the target map
        comp.AvailableGrids.Clear();
        comp.SelectedGridUid = null;
        comp.SelectedGridName = null;

        var targetMapXform = Transform(mapUid);
        var mapId = targetMapXform.MapID;

        if (mapId != MapId.Nullspace)
        {
            var grids = new List<Entity<MapGridComponent>>();
            _mapManager.FindGridsIntersecting(mapId, new Box2(-1e10f, -1e10f, 1e10f, 1e10f), ref grids, includeMap: false);

            foreach (var grid in grids)
            {
                var name = MetaData(grid.Owner).EntityName;
                if (!string.IsNullOrEmpty(name))
                    comp.AvailableGrids.Add(name);
            }
        }

        var ts = DateTime.Now.ToString("HH:mm:ss");
        comp.LogEntries.Add($"[{ts}] [ДИСК]: Считаны данные сектора: \"{comp.TargetMapName}\". Гридов на карте: {comp.AvailableGrids.Count}.");

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
        comp.AvailableGrids.Clear();
        comp.SelectedGridUid = null;
        comp.SelectedGridName = null;

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
            if (!comp.IsOnCooldown)
                continue;

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

        // Build radar state for the target map (or console's own map if no disk)
        NavInterfaceState? radarState = null;
        var docks = _shuttleConsole.GetAllDocks();

        if (comp.TargetMapUid != null)
        {
            // Radar centered on the target map — use first grid as reference
            var targetMapXform = Transform(comp.TargetMapUid.Value);
            var mapId = targetMapXform.MapID;

            if (mapId != MapId.Nullspace)
            {
                var grids = new List<Entity<MapGridComponent>>();
                _mapManager.FindGridsIntersecting(mapId, new Box2(-1e10f, -1e10f, 1e10f, 1e10f), ref grids, includeMap: false);

                if (grids.Count > 0)
                {
                    var refGrid = grids[0];
                    var gridXform = Transform(refGrid.Owner);
                    var gridCenter = gridXform.Coordinates;
                    radarState = new NavInterfaceState(RadarMaxRange, GetNetCoordinates(gridCenter), Angle.Zero, docks);
                }
            }
        }

        if (radarState == null)
        {
            // Fallback: radar centered on console
            var consoleXform = Transform(uid);
            radarState = new NavInterfaceState(RadarMaxRange, GetNetCoordinates(consoleXform.Coordinates), consoleXform.LocalRotation, docks);
        }

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
            comp.AvailableGrids,
            radarState,
            comp.SelectedGridUid != null ? GetNetEntity(comp.SelectedGridUid.Value) : null,
            comp.SelectedGridName);

        _ui.SetUiState(uid, BSAConsoleUiKey.Key, state);
    }
}
