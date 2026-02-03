using System.Numerics;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Parallax.Biomes.Layers;
using Content.Shared.Parallax.Biomes.Markers;
using Robust.Shared.Console;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using ChunkIndicesEnumerator = Robust.Shared.Map.Enumerators.ChunkIndicesEnumerator;

namespace Content.Server.Parallax;

public sealed partial class BiomeSystem
{
    private void InitializeCommands()
    {
        _console.RegisterCommand("biome_clear", Loc.GetString("cmd-biome_clear-desc"), Loc.GetString("cmd-biome_clear-help"), BiomeClearCallback, BiomeClearCallbackHelper);
        _console.RegisterCommand("biome_addlayer", Loc.GetString("cmd-biome_addlayer-desc"), Loc.GetString("cmd-biome_addlayer-help"), AddLayerCallback, AddLayerCallbackHelp);
        _console.RegisterCommand("biome_addmarkerlayer", Loc.GetString("cmd-biome_addmarkerlayer-desc"), Loc.GetString("cmd-biome_addmarkerlayer-desc"), AddMarkerLayerCallback, AddMarkerLayerCallbackHelper);
        _console.RegisterCommand("biome_generate", "Generates a static planet map and saves it to file", "biome_generate <biomeTemplate> <size> <savePath> [seed]", GeneratePlanetCallback, GeneratePlanetCallbackHelper);
    }

    [AdminCommand(AdminFlags.Fun)]
    private void BiomeClearCallback(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 1)
        {
            return;
        }

        int.TryParse(args[0], out var mapInt);
        var mapId = new MapId(mapInt);
        var mapUid = _mapSystem.GetMapOrInvalid(mapId);

        if (_mapSystem.MapExists(mapId) ||
            !TryComp<BiomeComponent>(mapUid, out var biome))
        {
            return;
        }

        ClearTemplate(mapUid, biome);
    }

    private CompletionResult BiomeClearCallbackHelper(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.Components<BiomeComponent>(args[0], EntityManager), "Biome");
        }

        return CompletionResult.Empty;
    }

    [AdminCommand(AdminFlags.Fun)]
    private void AddLayerCallback(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length < 3 || args.Length > 4)
        {
            return;
        }

        if (!int.TryParse(args[0], out var mapInt))
        {
            return;
        }

        var mapId = new MapId(mapInt);
        var mapUid = _mapSystem.GetMapOrInvalid(mapId);

        if (!_mapSystem.MapExists(mapId) || !TryComp<BiomeComponent>(mapUid, out var biome))
        {
            return;
        }

        if (!ProtoManager.TryIndex<BiomeTemplatePrototype>(args[1], out var template))
        {
            return;
        }

        var offset = 0;

        if (args.Length == 4)
        {
            int.TryParse(args[3], out offset);
        }

        AddTemplate(mapUid, biome, args[2], template, offset);
    }

    private CompletionResult AddLayerCallbackHelp(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.MapIds(EntityManager), "Map ID");
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<BiomeTemplatePrototype>(proto: ProtoManager), "Biome template");
        }

        if (args.Length == 3)
        {
            if (int.TryParse(args[0], out var mapInt))
            {
                var mapId = new MapId(mapInt);

                if (TryComp<BiomeComponent>(_mapSystem.GetMapOrInvalid(mapId), out var biome))
                {
                    var results = new List<string>();

                    foreach (var layer in biome.Layers)
                    {
                        if (layer is not BiomeDummyLayer dummy)
                            continue;

                        results.Add(dummy.ID);
                    }

                    return CompletionResult.FromHintOptions(results, "Dummy layer ID");
                }
            }
        }

        if (args.Length == 4)
        {
            return CompletionResult.FromHint("Seed offset");
        }

        return CompletionResult.Empty;
    }

    [AdminCommand(AdminFlags.Fun)]
    private void AddMarkerLayerCallback(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 2)
        {
            return;
        }

        if (!int.TryParse(args[0], out var mapInt))
        {
            return;
        }

        var mapId = new MapId(mapInt);

        if (!_mapSystem.MapExists(mapId) || !TryComp<BiomeComponent>(_mapSystem.GetMapOrInvalid(mapId), out var biome))
        {
            return;
        }

        if (!ProtoManager.HasIndex<BiomeMarkerLayerPrototype>(args[1]))
        {
            return;
        }

        if (!biome.MarkerLayers.Add(args[1]))
        {
            return;
        }

        biome.ForcedMarkerLayers.Add(args[1]);
    }

    private CompletionResult AddMarkerLayerCallbackHelper(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var allQuery = AllEntityQuery<MapComponent, BiomeComponent>();
            var options = new List<CompletionOption>();

            while (allQuery.MoveNext(out var mapComp, out _))
            {
                options.Add(new CompletionOption(mapComp.MapId.ToString()));
            }

            return CompletionResult.FromHintOptions(options, "Biome");
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<BiomeMarkerLayerPrototype>(proto: ProtoManager), "Marker");
        }

        return CompletionResult.Empty;
    }

    [AdminCommand(AdminFlags.Mapping)]
    private void GeneratePlanetCallback(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length < 3 || args.Length > 4)
        {
            shell.WriteError("Usage: biome_generate <biomeTemplate> <size> <savePath> [seed]");
            return;
        }

        if (!ProtoManager.TryIndex<BiomeTemplatePrototype>(args[0], out var template))
        {
            shell.WriteError($"Unknown biome template: {args[0]}");
            return;
        }

        if (!int.TryParse(args[1], out var size) || size <= 0)
        {
            shell.WriteError($"Invalid size: {args[1]}");
            return;
        }

        var savePath = args[2];

        int? seed = null;
        if (args.Length == 4)
        {
            if (!int.TryParse(args[3], out var parsedSeed))
            {
                shell.WriteError($"Invalid seed: {args[3]}");
                return;
            }
            seed = parsedSeed;
        }

        shell.WriteLine($"Creating planet {size}x{size} with biome '{args[0]}'...");

        // Create map without MapInit so it saves as uninitialized
        var mapUid = _mapSystem.CreateMap(out var mapId, runMapInit: false);

        // Manually setup biome and grid without triggering MapInit
        EnsureComp<MapGridComponent>(mapUid);
        var biome = EntityManager.ComponentFactory.GetComponent<BiomeComponent>();
        seed ??= _random.Next();
        SetSeed(mapUid, biome, seed.Value, false);
        SetTemplate(mapUid, biome, template, false);
        AddComp(mapUid, biome, true);

        var grid = Comp<MapGridComponent>(mapUid);

        var halfSize = size / 2f;
        var area = new Box2(-halfSize, -halfSize, halfSize, halfSize);

        // Populate active chunks for the entire area
        var activeSet = _tilePool.Get();
        _activeChunks[biome] = activeSet;
        _markerChunks.GetOrNew(biome);

        var enumerator = new ChunkIndicesEnumerator(area, ChunkSize);
        while (enumerator.MoveNext(out var chunkOrigin))
        {
            activeSet.Add(chunkOrigin.Value * ChunkSize);
        }

        // Preload marker chunks (ores, mobs, etc.)
        Preload(mapUid, biome, area);

        // Force load all chunks
        LoadChunks(biome, mapUid, grid, biome.Seed);

        shell.WriteLine($"Loaded {activeSet.Count} chunks.");

        // Cleanup temporary state
        _tilePool.Return(activeSet);
        _activeChunks.Remove(biome);
        _markerChunks.Remove(biome);

        // Remove biome component so no further dynamic generation happens
        RemComp<BiomeComponent>(mapUid);

        // Save the map
        var mapLoader = EntityManager.System<MapLoaderSystem>();
        if (mapLoader.TrySaveMap(mapId, new ResPath(savePath)))
            shell.WriteLine($"Planet saved to {savePath}");
        else
            shell.WriteError($"Failed to save map to {savePath}");

        // Delete the temporary map
        _mapSystem.DeleteMap(mapId);
        shell.WriteLine("Done. Temporary map deleted.");
    }

    private CompletionResult GeneratePlanetCallbackHelper(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<BiomeTemplatePrototype>(proto: ProtoManager), "Biome template");
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHint("Size (e.g. 300)");
        }

        if (args.Length == 3)
        {
            return CompletionResult.FromHint("Save path (e.g. /Maps/planet.yml)");
        }

        if (args.Length == 4)
        {
            return CompletionResult.FromHint("Seed (optional)");
        }

        return CompletionResult.Empty;
    }
}
