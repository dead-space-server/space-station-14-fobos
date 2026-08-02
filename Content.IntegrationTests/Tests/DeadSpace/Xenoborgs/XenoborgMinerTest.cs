// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Content.Server.Antag.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.DeadSpace.Lavaland.Components;
using Content.Server.DeadSpace.Xenoborgs.Components;
using Content.Server.Tiles;
using Content.Shared.Atmos;
using Content.Shared.Chasm;
using Content.Shared.Containers;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Power.Components;
using Content.Shared.Projectiles;
using Content.Shared.Research.Prototypes;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.EnergySword;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Wieldable.Components;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests.DeadSpace.Xenoborgs;

[TestFixture]
public sealed class XenoborgMinerTest
{
    private static readonly Vector2 CoreMarker = new(0.5f, 0.5f);

    private static readonly Vector2[] XenoborgMarkers =
    [
        new(-0.5f, 0.5f),
        new(1.5f, 0.5f),
        new(-0.5f, 1.5f),
        new(0.5f, 1.5f),
        new(1.5f, 1.5f),
    ];

    private static readonly string[] RoundStartXenoborgs =
    [
        "XenoborgEngi",
        "XenoborgHeavy",
        "XenoborgScout",
        "XenoborgStealth",
        "XenoborgMiner",
    ];

    [Test]
    public async Task MothershipAndMinerContract()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = false,
            Dirty = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var protoMan = server.ProtoMan;
        var componentFactory = server.ResolveDependency<IComponentFactory>();
        var resourceManager = server.ResolveDependency<IResourceManager>();
        var mapManager = server.ResolveDependency<IMapManager>();
        var mapSystem = server.System<SharedMapSystem>();
        var mapLoader = server.System<MapLoaderSystem>();
        var turf = server.System<TurfSystem>();
        var atmosphere = server.System<AtmosphereSystem>();
        var transform = server.System<SharedTransformSystem>();
        var useDelay = server.System<UseDelaySystem>();
        var mapPath = new ResPath("/Maps/Shuttles/mothership.yml");

        EntityUid gridUid = default;
        MapId mapId = default;

        await server.WaitPost(() =>
        {
            using (var reader = new StreamReader(resourceManager.ContentFileRead(mapPath)))
            {
                var yaml = reader.ReadToEnd();
                Assert.Multiple(() =>
                {
                    Assert.That(yaml, Does.Match(@"(?m)^  format: 7\r?$"));
                    Assert.That(yaml, Does.Match(@"(?m)^  category: Grid\r?$"));
                    Assert.That(yaml, Does.Match(@"(?m)^  entityCount: 1210\r?$"));
                    Assert.That(yaml, Does.Match(@"(?ms)^maps: \[\]\r?\ngrids:\r?\n- 1\r?$"));
                });
            }

            mapSystem.CreateMap(out mapId);
            Assert.That(mapLoader.TryLoadGrid(mapId, mapPath, out var loadedGrid), Is.True);
            Assert.That(loadedGrid, Is.Not.Null);
            gridUid = loadedGrid!.Value.Owner;
        });

        await server.WaitAssertion(() =>
        {
            Assert.That(mapManager.GetAllGrids(mapId).Count(), Is.EqualTo(1));
            AssertMapMarkers(entMan, turf, gridUid);
            AssertRoundStartRules(protoMan, componentFactory);
            AssertMinerPrototypes(protoMan, componentFactory);
            AssertUpgradePrototypes(protoMan, componentFactory);
            AssertRecipes(protoMan);
        });

        await server.WaitAssertion(() =>
        {
            AssertPortalGunBranches(
                entMan,
                mapSystem,
                turf,
                atmosphere,
                useDelay,
                gridUid);

            AssertJaunterBranches(entMan, transform, useDelay, gridUid);
        });

        await pair.CleanReturnAsync();
    }

    private static void AssertMapMarkers(IEntityManager entMan, TurfSystem turf, EntityUid gridUid)
    {
        var coreMarkers = new List<Vector2>();
        var xenoborgMarkers = new List<Vector2>();
        var query = entMan.AllEntityQueryEnumerator<TransformComponent, MetaDataComponent>();
        while (query.MoveNext(out var xform, out var metadata))
        {
            if (xform.GridUid != gridUid || metadata.EntityPrototype == null)
                continue;

            switch (metadata.EntityPrototype.ID)
            {
                case "SpawnPointMothershipCore":
                    coreMarkers.Add(xform.LocalPosition);
                    break;
                case "SpawnPointXenoborg":
                    xenoborgMarkers.Add(xform.LocalPosition);
                    break;
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(coreMarkers, Is.EquivalentTo(new[] { CoreMarker }));
            Assert.That(xenoborgMarkers, Is.EquivalentTo(XenoborgMarkers));
        });

        foreach (var position in XenoborgMarkers.Append(CoreMarker))
        {
            Assert.That(turf.TryGetTileRef(new EntityCoordinates(gridUid, position), out var tile), Is.True);
            Assert.That(turf.IsSpace(tile!.Value), Is.False, $"Marker at {position} is in space.");
            Assert.That(
                turf.IsTileBlocked(tile.Value, CollisionGroup.MobMask),
                Is.False,
                $"Marker at {position} is blocked.");
        }
    }

    private static void AssertRoundStartRules(IPrototypeManager protoMan, IComponentFactory componentFactory)
    {
        foreach (var ruleId in new[] { "Xenoborgs", "SubXenoborgs" })
        {
            var rule = protoMan.Index<EntityPrototype>(ruleId);
            Assert.That(
                rule.TryGetComponent<AntagMultipleRoleSpawnerComponent>(out var spawner, componentFactory),
                Is.True);
            Assert.That(spawner!.PickAndTake, Is.True);

            var prototypes = spawner.AntagRoleToPrototypes
                .Single(pair => pair.Key.ToString() == "Xenoborg")
                .Value
                .Select(id => id.ToString())
                .ToArray();
            Assert.That(prototypes, Is.EquivalentTo(RoundStartXenoborgs));
            Assert.That(prototypes.Distinct().Count(), Is.EqualTo(5));

            Assert.That(rule.TryGetComponent<AntagSelectionComponent>(out var selection, componentFactory), Is.True);
            var definition = selection!.Definitions.Single(definition =>
                definition.PrefRoles.Any(role => role.ToString() == "Xenoborg"));
            Assert.Multiple(() =>
            {
                Assert.That(definition.Min, Is.EqualTo(5));
                Assert.That(definition.Max, Is.EqualTo(5));
            });
        }

        Assert.That(protoMan.TryIndex<EntityPrototype>("XenoborgMinerPrinted", out _), Is.False);
    }

    private static void AssertMinerPrototypes(IPrototypeManager protoMan, IComponentFactory componentFactory)
    {
        var miner = protoMan.Index<EntityPrototype>("XenoborgMiner");
        Assert.That(miner.TryGetComponent<BorgChassisComponent>(out var chassis, componentFactory), Is.True);
        Assert.That(miner.TryGetComponent<ContainerFillComponent>(out var fill, componentFactory), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(chassis!.MaxModules, Is.EqualTo(4));
            Assert.That(
                chassis.ModuleWhitelist?.Tags?.Select(tag => tag.ToString()),
                Is.EquivalentTo(new[] { "XenoborgModuleMiner" }));
            Assert.That(
                fill!.Containers["borg_module"],
                Is.EqualTo(new[]
                {
                    "XenoborgModuleBasic",
                    "XenoborgModuleTool",
                    "XenoborgModuleSpaceMovement",
                    "XenoborgModuleMiner",
                }));
        });

        foreach (var moduleId in new[]
                 {
                     "XenoborgModuleBasic",
                     "XenoborgModuleTool",
                     "XenoborgModuleSpaceMovement",
                     "XenoborgModuleMiner",
                     "XenoborgModuleAdvancedMiner",
                 })
        {
            var module = protoMan.Index<EntityPrototype>(moduleId);
            Assert.That(module.TryGetComponent<TagComponent>(out var tags, componentFactory), Is.True);
            Assert.That(tags!.Tags.Select(tag => tag.ToString()), Does.Contain("XenoborgModuleMiner"));
        }

        var foreignModule = protoMan.Index<EntityPrototype>("XenoborgModuleHeavyLaser");
        Assert.That(foreignModule.TryGetComponent<TagComponent>(out var foreignTags, componentFactory), Is.True);
        Assert.That(foreignTags!.Tags.Select(tag => tag.ToString()), Does.Not.Contain("XenoborgModuleMiner"));

        AssertModuleHands(
            protoMan,
            componentFactory,
            "XenoborgModuleMiner",
            [
                "XenoborgMinerPickaxe",
                "WeaponProtoKineticAccelerator",
                "WeaponDisablerXenoborgMiner",
                "WeaponXenoborgPortalGun",
                "OreBag",
                "XenoborgJaunter",
            ]);
        AssertModuleHands(
            protoMan,
            componentFactory,
            "XenoborgModuleAdvancedMiner",
            [
                "XenoborgMinerDiamondPickaxe",
                "WeaponProtoKineticAccelerator",
                "WeaponDisablerSMGXenoborgMiner",
                "WeaponXenoborgPortalGun",
                "OreBagOfHolding",
                "AdvancedMineralScannerUnpowered",
                "XenoborgJaunterAdvanced",
            ]);

        AssertPickaxe(protoMan, componentFactory, "XenoborgMinerPickaxe", 13, 28, null);
        AssertPickaxe(protoMan, componentFactory, "XenoborgMinerDiamondPickaxe", 18, 43, (4, 2, 2));

        var disabler = GetComponent<BatterySelfRechargerComponent>(
            protoMan,
            componentFactory,
            "WeaponDisablerXenoborgMiner");
        var disablerSmg = GetComponent<BatterySelfRechargerComponent>(
            protoMan,
            componentFactory,
            "WeaponDisablerSMGXenoborgMiner");
        Assert.Multiple(() =>
        {
            Assert.That(disabler.AutoRechargeRate, Is.EqualTo(26.6667f).Within(0.0001f));
            Assert.That(disablerSmg.AutoRechargeRate, Is.EqualTo(32f));
            Assert.That(
                GetComponent<UseDelayComponent>(protoMan, componentFactory, "XenoborgJaunter").Delay.TotalSeconds,
                Is.EqualTo(150));
            Assert.That(
                GetComponent<UseDelayComponent>(protoMan, componentFactory, "XenoborgJaunterAdvanced").Delay.TotalSeconds,
                Is.EqualTo(120));
        });
    }

    private static void AssertUpgradePrototypes(IPrototypeManager protoMan, IComponentFactory componentFactory)
    {
        var sword = GetComponent<EnergySwordComponent>(
            protoMan,
            componentFactory,
            "XenoborgEnergySwordBlue");
        Assert.That(sword.ColorOptions, Is.EqualTo(new[] { Color.FromHex("#2288ff") }));

        var cannonAmmo = GetComponent<BatteryAmmoProviderComponent>(
            protoMan,
            componentFactory,
            "WeaponLaserCannonXenoborg");
        Assert.That(cannonAmmo.Prototype.ToString(), Is.EqualTo("RedHeavyLaserXenoborg"));
        Assert.Multiple(() =>
        {
            Assert.That(
                GetHeatDamage(protoMan, componentFactory, "RedHeavyLaserXenoborg"),
                Is.EqualTo(FixedPoint2.New(25)));
            Assert.That(
                GetHeatDamage(protoMan, componentFactory, "RedHeavyLaser"),
                Is.EqualTo(FixedPoint2.New(20)));
            Assert.That(
                GetComponent<ProjectileComponent>(protoMan, componentFactory, "XenoborgPortalBolt")
                    .Damage.GetTotal(),
                Is.EqualTo(FixedPoint2.Zero));
        });
    }

    private static void AssertRecipes(IPrototypeManager protoMan)
    {
        AssertRecipe(protoMan, "XenoborgModuleDoorControlRecipe", new Dictionary<string, int>
        {
            ["Steel"] = 2000,
            ["Glass"] = 1500,
            ["Plastic"] = 1000,
        });
        AssertRecipe(protoMan, "XenoborgModuleHeavyLaserRecipe", new Dictionary<string, int>
        {
            ["Steel"] = 2000,
            ["Glass"] = 1500,
            ["Plasma"] = 1000,
            ["Plastic"] = 500,
            ["Diamond"] = 25,
        });
        AssertRecipe(protoMan, "XenoborgModuleEnergySwordRecipe", new Dictionary<string, int>
        {
            ["Steel"] = 1500,
            ["Glass"] = 1000,
            ["Plasma"] = 1500,
            ["Plastic"] = 1000,
            ["Diamond"] = 25,
        });
        AssertRecipe(protoMan, "XenoborgModuleSuperCloakDeviceRecipe", new Dictionary<string, int>
        {
            ["Steel"] = 1000,
            ["Glass"] = 2000,
            ["Plasma"] = 1000,
            ["Plastic"] = 1500,
            ["Diamond"] = 25,
        });
        AssertRecipe(protoMan, "XenoborgModuleAdvancedMinerRecipe", new Dictionary<string, int>
        {
            ["Steel"] = 2500,
            ["Glass"] = 1500,
            ["Plasma"] = 500,
            ["Plastic"] = 1000,
            ["Diamond"] = 1000,
        });
    }

    private static void AssertPortalGunBranches(
        IEntityManager entMan,
        SharedMapSystem mapSystem,
        TurfSystem turf,
        AtmosphereSystem atmosphere,
        UseDelaySystem useDelay,
        EntityUid gridUid)
    {
        var grid = entMan.GetComponent<MapGridComponent>(gridUid);

        TimeSpan FireAt(EntityUid target)
        {
            var targetCoordinates = entMan.GetComponent<TransformComponent>(target).Coordinates;
            var gun = entMan.SpawnEntity("WeaponXenoborgPortalGun", targetCoordinates);
            var projectile = entMan.SpawnEntity("XenoborgPortalBolt", targetCoordinates);
            entMan.EventBus.RaiseLocalEvent(gun, new AmmoShotEvent
            {
                FiredProjectiles = [projectile],
            });

            var hit = new ProjectileHitEvent(new DamageSpecifier(), target);
            entMan.EventBus.RaiseLocalEvent(projectile, ref hit);

            Assert.That(entMan.TryGetComponent<UseDelayComponent>(gun, out var delay), Is.True);
            Assert.That(useDelay.TryGetDelayInfo((gun, delay), out var info), Is.True);
            return info!.Length;
        }

        var liveTarget = entMan.SpawnEntity(
            "MobHuman",
            new EntityCoordinates(gridUid, XenoborgMarkers[0]));
        SeedSafeAtmosphere(entMan, mapSystem, turf, atmosphere, gridUid, grid, liveTarget);
        var safeDestinations = GetSafeDestinations(
            entMan,
            mapSystem,
            turf,
            atmosphere,
            gridUid,
            grid,
            liveTarget);
        var liveOrigin = entMan.GetComponent<TransformComponent>(liveTarget).LocalPosition;
        Assert.That(FireAt(liveTarget).TotalSeconds, Is.EqualTo(120));

        var liveXform = entMan.GetComponent<TransformComponent>(liveTarget);
        Assert.Multiple(() =>
        {
            Assert.That(liveXform.GridUid, Is.EqualTo(gridUid));
            Assert.That(liveXform.LocalPosition, Is.Not.EqualTo(liveOrigin));
        });
        AssertSafeTile(entMan, turf, liveTarget, safeDestinations);

        var stunnedTarget = entMan.SpawnEntity(
            "MobHuman",
            new EntityCoordinates(gridUid, XenoborgMarkers[1]));
        entMan.EnsureComponent<StunnedComponent>(stunnedTarget);
        Assert.That(FireAt(stunnedTarget).TotalSeconds, Is.EqualTo(45));
        Assert.That(entMan.GetComponent<TransformComponent>(stunnedTarget).GridUid, Is.EqualTo(gridUid));

        var nonLivingTarget = entMan.SpawnEntity(
            "Crowbar",
            new EntityCoordinates(gridUid, XenoborgMarkers[2]));
        var nonLivingOrigin = entMan.GetComponent<TransformComponent>(nonLivingTarget).LocalPosition;
        Assert.That(FireAt(nonLivingTarget).TotalSeconds, Is.EqualTo(3));
        Assert.That(
            entMan.GetComponent<TransformComponent>(nonLivingTarget).LocalPosition,
            Is.EqualTo(nonLivingOrigin));
    }

    private static void SeedSafeAtmosphere(
        IEntityManager entMan,
        SharedMapSystem mapSystem,
        TurfSystem turf,
        AtmosphereSystem atmosphere,
        EntityUid gridUid,
        MapGridComponent grid,
        EntityUid target)
    {
        var xform = entMan.GetComponent<TransformComponent>(target);
        var physics = entMan.GetComponent<PhysicsComponent>(target);
        Assert.That(xform.MapUid, Is.Not.Null);

        foreach (var tile in mapSystem.GetAllTiles(gridUid, grid))
        {
            if (tile.Tile.IsEmpty ||
                turf.IsSpace(tile) ||
                turf.IsTileBlocked(tile, (CollisionGroup) physics.CollisionMask))
            {
                continue;
            }

            var mixture = atmosphere.GetTileMixture(gridUid, xform.MapUid!.Value, tile.GridIndices, true);
            if (mixture == null || mixture.Immutable)
                continue;

            for (var gas = 0; gas < Atmospherics.AdjustedNumberOfGases; gas++)
                mixture.SetMoles(gas, 0f);

            var standardMoles = Atmospherics.OneAtmosphere * mixture.Volume /
                                (Atmospherics.T20C * Atmospherics.R);
            mixture.SetMoles(Gas.Oxygen, standardMoles * Atmospherics.OxygenStandard);
            mixture.SetMoles(Gas.Nitrogen, standardMoles * Atmospherics.NitrogenStandard);
            mixture.Temperature = Atmospherics.T20C;
        }
    }

    private static HashSet<Vector2i> GetSafeDestinations(
        IEntityManager entMan,
        SharedMapSystem mapSystem,
        TurfSystem turf,
        AtmosphereSystem atmosphere,
        EntityUid gridUid,
        MapGridComponent grid,
        EntityUid target)
    {
        var xform = entMan.GetComponent<TransformComponent>(target);
        var physics = entMan.GetComponent<PhysicsComponent>(target);
        Assert.That(xform.MapUid, Is.Not.Null);

        var total = 0;
        var nonEmpty = 0;
        var nonSpace = 0;
        var unblocked = 0;
        var safeAtmosphere = 0;
        var safeTiles = new HashSet<Vector2i>();
        foreach (var tile in mapSystem.GetAllTiles(gridUid, grid))
        {
            total++;
            if (tile.Tile.IsEmpty)
                continue;
            nonEmpty++;
            if (turf.IsSpace(tile))
                continue;
            nonSpace++;
            if (turf.IsTileBlocked(tile, (CollisionGroup) physics.CollisionMask))
                continue;
            unblocked++;
            if (!atmosphere.IsTileMixtureProbablySafe(gridUid, xform.MapUid!.Value, tile.GridIndices))
                continue;
            safeAtmosphere++;

            var anchored = mapSystem.GetAnchoredEntitiesEnumerator(gridUid, grid, tile.GridIndices);
            var hazard = false;
            while (anchored.MoveNext(out var anchoredUid))
            {
                if (anchoredUid != null &&
                    (entMan.HasComponent<ChasmComponent>(anchoredUid.Value) ||
                     entMan.HasComponent<TileEntityEffectComponent>(anchoredUid.Value)))
                {
                    hazard = true;
                    break;
                }
            }

            if (!hazard)
                safeTiles.Add(tile.GridIndices);
        }

        Assert.That(
            safeTiles.Count,
            Is.GreaterThan(0),
            $"No safe destination: total={total}, nonEmpty={nonEmpty}, nonSpace={nonSpace}, " +
            $"unblocked={unblocked}, safeAtmosphere={safeAtmosphere}, safe={safeTiles.Count}.");
        return safeTiles;
    }

    private static void AssertSafeTile(
        IEntityManager entMan,
        TurfSystem turf,
        EntityUid target,
        HashSet<Vector2i> safeDestinations)
    {
        var xform = entMan.GetComponent<TransformComponent>(target);
        Assert.That(xform.MapUid, Is.Not.Null);
        Assert.That(turf.TryGetTileRef(xform.Coordinates, out var tile), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(turf.IsSpace(tile!.Value), Is.False);
            Assert.That(safeDestinations, Does.Contain(tile.Value.GridIndices));
        });
    }

    private static void AssertJaunterBranches(
        IEntityManager entMan,
        SharedTransformSystem transform,
        UseDelaySystem useDelay,
        EntityUid gridUid)
    {
        var user = entMan.SpawnEntity("MobHuman", new EntityCoordinates(gridUid, XenoborgMarkers[3]));
        var jaunter = entMan.SpawnEntity("XenoborgJaunter", entMan.GetComponent<TransformComponent>(user).Coordinates);
        var jaunterDelay = entMan.GetComponent<UseDelayComponent>(jaunter);
        Assert.That(useDelay.TryGetDelayInfo((jaunter, jaunterDelay), out var initialDelay), Is.True);
        var initialEndTime = initialDelay!.EndTime;
        var initialCoordinates = entMan.GetComponent<TransformComponent>(user).Coordinates;

        entMan.EventBus.RaiseLocalEvent(jaunter, new UseInHandEvent(user));

        Assert.That(entMan.GetComponent<TransformComponent>(user).Coordinates, Is.EqualTo(initialCoordinates));
        Assert.That(useDelay.TryGetDelayInfo((jaunter, jaunterDelay), out var failedDelay), Is.True);
        Assert.That(failedDelay!.EndTime, Is.EqualTo(initialEndTime));

        var core = entMan.SpawnEntity("MothershipCore", new EntityCoordinates(gridUid, CoreMarker));
        entMan.EventBus.RaiseLocalEvent(jaunter, new UseInHandEvent(user));

        var userXform = entMan.GetComponent<TransformComponent>(user);
        Assert.Multiple(() =>
        {
            Assert.That(userXform.GridUid, Is.EqualTo(gridUid));
            Assert.That(
                Vector2.Distance(
                    transform.GetMapCoordinates(user).Position,
                    transform.GetMapCoordinates(core).Position),
                Is.LessThanOrEqualTo(5.01f));
            Assert.That(useDelay.IsDelayed((jaunter, jaunterDelay)), Is.True);
        });
        Assert.That(useDelay.TryGetDelayInfo((jaunter, jaunterDelay), out var successfulDelay), Is.True);
        Assert.That(successfulDelay!.Length.TotalSeconds, Is.EqualTo(150));
    }

    private static void AssertModuleHands(
        IPrototypeManager protoMan,
        IComponentFactory componentFactory,
        string moduleId,
        string[] expected)
    {
        var module = GetComponent<ItemBorgModuleComponent>(protoMan, componentFactory, moduleId);
        Assert.That(
            module.Hands.Select(hand => hand.Item?.ToString()),
            Is.EqualTo(expected));
    }

    private static void AssertPickaxe(
        IPrototypeManager protoMan,
        IComponentFactory componentFactory,
        string prototypeId,
        int brute,
        int structural,
        (int Yield, int Cleave, int CleaveYield)? mining)
    {
        var prototype = protoMan.Index<EntityPrototype>(prototypeId);
        var melee = GetComponent<MeleeWeaponComponent>(protoMan, componentFactory, prototypeId);
        var bruteGroup = protoMan.Index<DamageGroupPrototype>("Brute");
        var bruteDamage = bruteGroup.DamageTypes.Aggregate(FixedPoint2.Zero, (current, type) =>
            current + melee.Damage.DamageDict.GetValueOrDefault(type));
        var hasWieldable = prototype.TryGetComponent<WieldableComponent>(out _, componentFactory);
        var requiresWield = prototype.TryGetComponent<MeleeRequiresWieldComponent>(out _, componentFactory);
        var hasWieldDamage = prototype.TryGetComponent<IncreaseDamageOnWieldComponent>(out _, componentFactory);

        Assert.Multiple(() =>
        {
            Assert.That(hasWieldable, Is.False);
            Assert.That(requiresWield, Is.False);
            Assert.That(hasWieldDamage, Is.False);
            Assert.That(bruteDamage, Is.EqualTo(FixedPoint2.New(brute)));
            Assert.That(
                melee.Damage.DamageDict.GetValueOrDefault("Structural"),
                Is.EqualTo(FixedPoint2.New(structural)));
        });

        if (mining is not { } expectedMining)
            return;

        var miningTool = GetComponent<LavalandMiningToolComponent>(
            protoMan,
            componentFactory,
            prototypeId);
        Assert.Multiple(() =>
        {
            Assert.That(miningTool.YieldMultiplier, Is.EqualTo(expectedMining.Yield));
            Assert.That(miningTool.CleaveTargets, Is.EqualTo(expectedMining.Cleave));
            Assert.That(miningTool.CleaveYieldMultiplier, Is.EqualTo(expectedMining.CleaveYield));
        });
    }

    private static FixedPoint2 GetHeatDamage(
        IPrototypeManager protoMan,
        IComponentFactory componentFactory,
        string prototypeId)
    {
        return GetComponent<ProjectileComponent>(protoMan, componentFactory, prototypeId)
            .Damage.DamageDict.GetValueOrDefault("Heat");
    }

    private static void AssertRecipe(
        IPrototypeManager protoMan,
        string recipeId,
        Dictionary<string, int> expected)
    {
        var recipe = protoMan.Index<LatheRecipePrototype>(recipeId);
        var materials = recipe.Materials.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);
        Assert.That(materials, Is.EqualTo(expected));
    }

    private static T GetComponent<T>(
        IPrototypeManager protoMan,
        IComponentFactory componentFactory,
        string prototypeId)
        where T : Component, new()
    {
        var prototype = protoMan.Index<EntityPrototype>(prototypeId);
        Assert.That(prototype.TryGetComponent<T>(out var component, componentFactory), Is.True);
        return component!;
    }
}
