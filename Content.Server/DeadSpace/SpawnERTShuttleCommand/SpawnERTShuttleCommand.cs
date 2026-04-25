// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Administration;
using Content.Server.RoundEnd;
using Content.Server.Shuttles;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Administration;
using Content.Shared.Tag;
using Robust.Shared.Console;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.SpawnERTShuttleCommand;

[AdminCommand(AdminFlags.Spawn)]
public sealed class SpawnERTShuttleCommand : LocalizedCommands
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private const string DockTagPrefix = "DockCentcommERT";

    public override string Command => "ert_spawn_shuttle";
    public override string Description => "Создаёт и стыкует к станции ЦК шаттл в заданный стык ОБР.";
    public override string Help => "ert_spawn_shuttle <шаттл> [dockTag]";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is < 1 or > 2)
        {
            shell.WriteError($"Usage: {Help}");
            return;
        }

        var roundEnd = _entityManager.System<RoundEndSystem>();
        var docking = _entityManager.System<DockingSystem>();
        var shuttleSystem = _entityManager.System<ShuttleSystem>();
        var mapLoader = _entityManager.System<MapLoaderSystem>();

        var centcommMap = roundEnd.GetCentcomm();
        var centcommGrid = roundEnd.GetCentcommGridEntity();

        if (!_entityManager.TryGetComponent(centcommMap, out MapComponent? mapComponent))
        {
            shell.WriteError("Ошибка: Не найден MapComponent у карты ЦК.");
            return;
        }

        if (_entityManager.Deleted(centcommGrid))
        {
            shell.WriteError("Ошибка: ЦК не существует или было удалено.");
            return;
        }

        if (!_prototypeManager.TryIndex(args[0], out ERTShuttlePrototype? shuttlePrototype))
        {
            shell.WriteError("Ошибка: Неверный аргумент.");
            return;
        }

        ProtoId<TagPrototype>? dockTag = args.Length == 2
            ? args[1]
            : shuttlePrototype.DockTag;

        if (dockTag != null && !_prototypeManager.HasIndex<TagPrototype>(dockTag.Value))
        {
            shell.WriteError($"Ошибка: Неверный тег стыка {dockTag.Value}.");
            return;
        }

        var dockGroups = GetDockGroups(centcommGrid.Value, docking, dockTag);

        if (dockGroups.Count == 0)
        {
            shell.WriteError(dockTag == null
                ? "Ошибка: На ЦК не найдены стыки ОБР."
                : $"Ошибка: На ЦК не найден стык ОБР с тегом {dockTag.Value}.");
            return;
        }

        if (dockTag != null)
        {
            if (IsDockGroupOccupied(dockGroups[0].Docks))
            {
                shell.WriteError($"Ошибка: Стык ОБР {dockTag.Value} занят.");
                return;
            }
        }
        else
        {
            dockGroups = dockGroups
                .Where(group => !IsDockGroupOccupied(group.Docks))
                .ToList();

            if (dockGroups.Count == 0)
            {
                shell.WriteError("Ошибка: Все стыки ОБР на ЦК заняты.");
                return;
            }
        }

        if (!mapLoader.TryLoadGrid(mapComponent.MapId, shuttlePrototype.Path, out var shuttle) ||
            shuttle == null ||
            _entityManager.Deleted(shuttle.Value.Owner))
        {
            shell.WriteError("Ошибка: Шаттл не существует или был удалён.");
            return;
        }

        var shuttleUid = shuttle.Value.Owner;

        if (!_entityManager.HasComponent<ShuttleComponent>(shuttleUid))
        {
            _entityManager.DeleteEntity(shuttleUid);
            shell.WriteError("Ошибка: Не найден ShuttleComponent у заспавненного шаттла.");
            return;
        }

        var shuttleDocks = docking.GetDocks(shuttleUid);
        if (shuttleDocks.Count == 0)
        {
            _entityManager.DeleteEntity(shuttleUid);
            shell.WriteError("Ошибка: Не найдены стыки у заспавненного шаттла.");
            return;
        }

        if (!_entityManager.TryGetComponent(shuttleUid, out TransformComponent? shuttleTransform))
        {
            _entityManager.DeleteEntity(shuttleUid);
            shell.WriteError("Ошибка: Не найден TransformComponent у заспавненного шаттла.");
            return;
        }

        if (!TryGetDockingConfig(shuttleUid, centcommGrid.Value, shuttleDocks, dockGroups, docking, out var config))
        {
            _entityManager.DeleteEntity(shuttleUid);
            shell.WriteError("Ошибка: Стыковка не выполнена.");
            return;
        }

        shuttleSystem.FTLDock((shuttleUid, shuttleTransform), config);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var shuttles = _prototypeManager.EnumeratePrototypes<ERTShuttlePrototype>()
                .Select(p => new CompletionOption(p.ID));

            return CompletionResult.FromHintOptions(shuttles, "<шаттл>");
        }

        if (args.Length == 2)
        {
            var dockTags = _prototypeManager.EnumeratePrototypes<TagPrototype>()
                .Where(p => IsErtShuttleDockTag(p.ID))
                .Select(p => new CompletionOption(p.ID));

            return CompletionResult.FromHintOptions(dockTags, "<dockTag>");
        }

        return CompletionResult.Empty;
    }

    private List<(ProtoId<TagPrototype> Tag, List<Entity<DockingComponent>> Docks)> GetDockGroups(
        EntityUid grid,
        DockingSystem docking,
        ProtoId<TagPrototype>? requestedTag)
    {
        var groups = new Dictionary<ProtoId<TagPrototype>, List<Entity<DockingComponent>>>();

        foreach (var dock in docking.GetDocks(grid))
        {
            if (!_entityManager.TryGetComponent(dock.Owner, out TagComponent? tagComponent))
            {
                continue;
            }

            var dockTags = requestedTag == null
                ? tagComponent.Tags.Where(tag => IsErtShuttleDockTag(tag.Id))
                : tagComponent.Tags.Where(tag => tag.Equals(requestedTag.Value));

            foreach (var currentTag in dockTags)
            {
                if (!groups.TryGetValue(currentTag, out var docks))
                {
                    docks = new List<Entity<DockingComponent>>();
                    groups.Add(currentTag, docks);
                }

                docks.Add(dock);
            }
        }

        return groups
            .Select(group => (group.Key, group.Value))
            .ToList();
    }

    private static bool IsDockGroupOccupied(List<Entity<DockingComponent>> docks)
    {
        return docks.Any(dock => dock.Comp.Docked);
    }

    private static bool IsErtShuttleDockTag(string tag)
    {
        return tag.StartsWith(DockTagPrefix) && tag != DockTagPrefix;
    }

    private static bool TryGetDockingConfig(
        EntityUid shuttleUid,
        EntityUid targetGrid,
        List<Entity<DockingComponent>> shuttleDocks,
        List<(ProtoId<TagPrototype> Tag, List<Entity<DockingComponent>> Docks)> targetGroups,
        DockingSystem docking,
        [NotNullWhen(true)] out DockingConfig? config)
    {
        foreach (var (_, targetDocks) in targetGroups)
        {
            config = docking.GetDockingConfig(shuttleUid, targetGrid, shuttleDocks, targetDocks);

            if (config != null && UsesAllTargetDocks(config, targetDocks))
                return true;
        }

        config = null;
        return false;
    }

    private static bool UsesAllTargetDocks(DockingConfig config, List<Entity<DockingComponent>> targetDocks)
    {
        return targetDocks.All(targetDock =>
            config.Docks.Any(docked => docked.DockBUid == targetDock.Owner));
    }
}
