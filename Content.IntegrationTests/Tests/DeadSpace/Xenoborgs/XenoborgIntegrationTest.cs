// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Content.Server.Antag.Components;
using Content.Server.DeadSpace.GPS.Components;
using Content.Server.DeadSpace.Lavaland.Components;
using Content.Server.DeadSpace.Xenoborgs.Components;
using Content.Server.Physics.Controllers;
using Content.Server.Shuttles.Components;
using Content.Server.Tiles;
using Content.Shared.Actions;
using Content.Shared.Chasm;
using Content.Shared.Containers;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DeadSpace.Xenoborgs;
using Content.Shared.DeviceLinking;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Movement.Components;
using Content.Shared.Physics;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Projectiles;
using Content.Shared.Research.Prototypes;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Stacks;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.EnergySword;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable.Components;
using Robust.Shared.ContentPack;
using Robust.Shared.Containers;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests.DeadSpace.Xenoborgs;

[TestFixture]
public sealed class XenoborgIntegrationTest
{
    private static readonly Vector2 CoreMarker = new(0.5f, 3.5f);
    private static readonly EntProtoId MothershipCorePrototype = "MothershipCore";
    private static readonly EntProtoId MothershipEyePrototype = "XenoborgMothershipEye";
    private static readonly EntProtoId MinerPrototype = "XenoborgMiner";
    private static readonly EntProtoId RemovedPrintedMinerPrototype = "XenoborgMinerPrinted";
    private static readonly EntProtoId HeavyLaserModulePrototype = "XenoborgModuleHeavyLaser";
    private static readonly EntProtoId PortalGunPrototype = "WeaponXenoborgPortalGun";
    private static readonly ProtoId<DamageGroupPrototype> BruteDamageGroup = "Brute";

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
    public async Task MothershipMapAndRoundstartConfiguration()
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
        var mapManager = server.ResolveDependency<IMapManager>();
        var turf = server.System<TurfSystem>();
        var (mapId, gridUid) = await LoadMothershipGrid(server, assertSerialization: true);

        await server.WaitAssertion(() =>
        {
            Assert.That(mapManager.GetAllGrids(mapId).Count(), Is.EqualTo(1));
            Assert.That(
                entMan.GetComponent<ShuttleComponent>(gridUid).FTLCooldownOverride,
                Is.EqualTo(TimeSpan.FromMinutes(3)));
            AssertMapMarkers(entMan, turf, gridUid);
            AssertMapLinks(entMan, gridUid);
            AssertMothershipResources(entMan, gridUid);
            AssertRoundStartRules(protoMan, componentFactory);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task MinerModulesAndTeleportation()
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
        var mapSystem = server.System<SharedMapSystem>();
        var turf = server.System<TurfSystem>();
        var transform = server.System<SharedTransformSystem>();
        var useDelay = server.System<UseDelaySystem>();
        var (_, gridUid) = await LoadMothershipGrid(server);

        await server.WaitAssertion(() =>
        {
            AssertMinerPrototypes(protoMan, componentFactory);
            AssertUpgradePrototypes(protoMan, componentFactory);
            AssertRecipes(protoMan);
            AssertJaunterBranches(entMan, transform, useDelay, gridUid);
            AssertPortalGunBranches(entMan, mapSystem, turf, useDelay, gridUid);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task MothershipCoreCollisionAndEyeMovement()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = false,
            Dirty = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var physics = server.System<SharedPhysicsSystem>();
        var mover = server.System<MoverController>();
        var transform = server.System<SharedTransformSystem>();
        var (_, gridUid) = await LoadMothershipGrid(server);

        (EntityUid Core, EntityUid Eye) eyeState = default;
        await server.WaitAssertion(() =>
        {
            eyeState = OpenMothershipEye(entMan, physics, transform, gridUid);
        });

        EntityUid collisionXenoborg = default;
        await server.WaitPost(() =>
        {
            collisionXenoborg = entMan.SpawnEntity(
                "XenoborgEngi",
                new EntityCoordinates(gridUid, new Vector2(0.5f, 2.5f)));
            var input = entMan.GetComponent<InputMoverComponent>(collisionXenoborg);
            mover.SetVelocityDirection((collisionXenoborg, input), Direction.North, ushort.MaxValue, true);

            var eyeInput = entMan.GetComponent<InputMoverComponent>(eyeState.Eye);
            mover.SetVelocityDirection((eyeState.Eye, eyeInput), Direction.East, ushort.MaxValue, true);
        });

        await server.WaitRunTicks(30);
        await server.WaitAssertion(() =>
        {
            var input = entMan.GetComponent<InputMoverComponent>(collisionXenoborg);
            mover.SetVelocityDirection((collisionXenoborg, input), Direction.North, ushort.MaxValue, false);
            var eyeInput = entMan.GetComponent<InputMoverComponent>(eyeState.Eye);
            mover.SetVelocityDirection((eyeState.Eye, eyeInput), Direction.East, ushort.MaxValue, false);

            Assert.Multiple(() =>
            {
                Assert.That(
                    entMan.GetComponent<TransformComponent>(collisionXenoborg).LocalPosition.Y,
                    Is.LessThanOrEqualTo(CoreMarker.Y - 0.79f),
                    "A moving xenoborg passed through the mothership core collider.");
                Assert.That(
                    entMan.GetComponent<TransformComponent>(eyeState.Eye).LocalPosition,
                    Is.Not.EqualTo(XenoborgMarkers[0]),
                    "The projected mothership eye did not move after receiving movement input.");
                Assert.That(
                    entMan.GetComponent<TransformComponent>(eyeState.Core).LocalPosition,
                    Is.EqualTo(CoreMarker));
            });

            CloseMothershipEye(entMan, eyeState.Core, eyeState.Eye);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task MothershipEyeInteractionsStayOnMothershipGrid()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = false,
            Dirty = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var mapManager = server.ResolveDependency<IMapManager>();
        var interaction = server.System<SharedInteractionSystem>();
        var physics = server.System<SharedPhysicsSystem>();
        var transform = server.System<SharedTransformSystem>();
        var (mapId, gridUid) = await LoadMothershipGrid(server);

        (EntityUid Core, EntityUid Eye) eyeState = default;
        await server.WaitAssertion(() =>
        {
            eyeState = OpenMothershipEye(entMan, physics, transform, gridUid);
        });

        await server.WaitAssertion(() =>
        {
            var airlock = FindMapEntity(
                entMan,
                gridUid,
                "AirlockXenoborgLocked",
                new Vector2(7.5f, 9.5f));
            var button = FindMapEntity(
                entMan,
                gridUid,
                "LockableButtonLawyer",
                new Vector2(9.5f, 8.5f));

            transform.SetCoordinates(eyeState.Eye, entMan.GetComponent<TransformComponent>(airlock).Coordinates);
            Assert.That(interaction.InteractionActivate(eyeState.Core, airlock), Is.True);

            transform.SetCoordinates(eyeState.Eye, entMan.GetComponent<TransformComponent>(button).Coordinates);
            Assert.That(interaction.InteractionActivate(eyeState.Core, button), Is.True);

            var foreignGrid = mapManager.CreateGridEntity(mapId);
            var map = server.System<SharedMapSystem>();
            map.SetTile(foreignGrid.Owner, foreignGrid.Comp, Vector2i.Zero, new Tile(1));
            transform.SetWorldPosition(foreignGrid.Owner, new Vector2(100f, 100f));
            var foreignButton = entMan.SpawnEntity(
                "SignalButton",
                new EntityCoordinates(foreignGrid.Owner, new Vector2(0.5f, 0.5f)));
            Assert.Multiple(() =>
            {
                Assert.That(
                    entMan.GetComponent<TransformComponent>(foreignButton).GridUid,
                    Is.EqualTo(foreignGrid.Owner));
                Assert.That(interaction.InRangeUnobstructed(eyeState.Core, foreignButton), Is.False);
                Assert.That(interaction.InteractionActivate(eyeState.Core, foreignButton), Is.False);
            });

            CloseMothershipEye(entMan, eyeState.Core, eyeState.Eye);
        });

        await pair.CleanReturnAsync();
    }

    private static async Task<(MapId MapId, EntityUid GridUid)> LoadMothershipGrid(
        RobustIntegrationTest.ServerIntegrationInstance server,
        bool assertSerialization = false)
    {
        var resourceManager = server.ResolveDependency<IResourceManager>();
        var mapSystem = server.System<SharedMapSystem>();
        var mapLoader = server.System<MapLoaderSystem>();
        var mapPath = new ResPath("/Maps/Shuttles/mothership.yml");
        EntityUid gridUid = default;
        MapId mapId = default;

        await server.WaitPost(() =>
        {
            if (assertSerialization)
            {
                using var reader = new StreamReader(resourceManager.ContentFileRead(mapPath));
                var yaml = reader.ReadToEnd();
                Assert.Multiple(() =>
                {
                    Assert.That(yaml, Does.Match(@"(?m)^  format: 7\r?$"));
                    Assert.That(yaml, Does.Match(@"(?m)^  category: Grid\r?$"));
                    Assert.That(yaml, Does.Match(@"(?m)^  entityCount: 1212\r?$"));
                    Assert.That(yaml, Does.Match(@"(?ms)^maps: \[\]\r?\ngrids:\r?\n- 1\r?$"));
                });
            }

            mapSystem.CreateMap(out mapId);
            Assert.That(mapLoader.TryLoadGrid(mapId, mapPath, out var loadedGrid), Is.True);
            Assert.That(loadedGrid, Is.Not.Null);
            gridUid = loadedGrid!.Value.Owner;
        });

        return (mapId, gridUid);
    }

    private static void AssertMapMarkers(IEntityManager entMan, TurfSystem turf, EntityUid gridUid)
    {
        var coreMarkers = new List<Vector2>();
        var xenoborgMarkers = new List<Vector2>();
        var mappedCores = new List<Vector2>();
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
                case "MothershipCore":
                    mappedCores.Add(xform.LocalPosition);
                    break;
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(coreMarkers, Is.EquivalentTo(new[] { CoreMarker }));
            Assert.That(xenoborgMarkers, Is.EquivalentTo(XenoborgMarkers));
            Assert.That(mappedCores, Is.Empty);
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

    private static void AssertMapLinks(IEntityManager entMan, EntityUid gridUid)
    {
        var eastAirlock = FindMapEntity(entMan, gridUid, "AirlockXenoborgLocked", new Vector2(7.5f, 9.5f));
        var eastBlastDoor = FindMapEntity(entMan, gridUid, "BlastDoorXenoborg", new Vector2(8.5f, 9.5f));
        var westBlastDoor = FindMapEntity(entMan, gridUid, "BlastDoorXenoborg", new Vector2(-7.5f, 9.5f));
        var eastButton = FindMapEntity(entMan, gridUid, "LockableButtonLawyer", new Vector2(9.5f, 8.5f));
        var westButton = FindMapEntity(entMan, gridUid, "LockableButtonLawyer", new Vector2(-8.5f, 8.5f));

        Assert.Multiple(() =>
        {
            AssertDeviceLink(entMan, eastAirlock, eastBlastDoor, "DoorStatus", "Toggle");
            AssertDeviceLink(entMan, eastButton, eastBlastDoor, "Pressed", "Toggle");
            AssertDeviceLink(entMan, westButton, westBlastDoor, "Pressed", "Toggle");
            Assert.That(entMan.HasComponent<DeviceLinkSinkComponent>(eastBlastDoor), Is.True);
            Assert.That(entMan.HasComponent<DeviceLinkSinkComponent>(westBlastDoor), Is.True);
        });
    }

    private static void AssertMothershipResources(IEntityManager entMan, EntityUid gridUid)
    {
        var crate = FindMapEntity(
            entMan,
            gridUid,
            "CrateCybersunSecure",
            new Vector2(-0.5f, 4.5f));
        var containers = entMan.GetComponent<ContainerManagerComponent>(crate);
        Assert.That(containers.Containers.TryGetValue("entity_storage", out var storage), Is.True);

        EntityUid FindStored(string prototype)
        {
            return storage!.ContainedEntities.Single(uid =>
                entMan.GetComponent<MetaDataComponent>(uid).EntityPrototype?.ID == prototype);
        }

        var plasma = FindStored("SheetPlasma1");
        var diamond = FindStored("MaterialDiamond1");
        Assert.Multiple(() =>
        {
            Assert.That(entMan.GetComponent<StackComponent>(plasma).Count, Is.EqualTo(20));
            Assert.That(entMan.GetComponent<StackComponent>(diamond).Count, Is.EqualTo(1));
        });
    }

    private static EntityUid FindMapEntity(
        IEntityManager entMan,
        EntityUid gridUid,
        string prototype,
        Vector2 position)
    {
        var found = new List<EntityUid>();
        var query = entMan.AllEntityQueryEnumerator<TransformComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out var xform, out var metadata))
        {
            if (xform.GridUid == gridUid &&
                xform.LocalPosition == position &&
                metadata.EntityPrototype?.ID == prototype)
            {
                found.Add(uid);
            }
        }

        Assert.That(found, Has.Count.EqualTo(1), $"Expected one {prototype} at {position}.");
        return found[0];
    }

    private static void AssertDeviceLink(
        IEntityManager entMan,
        EntityUid sourceUid,
        EntityUid sinkUid,
        string sourcePort,
        string sinkPort)
    {
        Assert.That(entMan.TryGetComponent<DeviceLinkSourceComponent>(sourceUid, out var source), Is.True);
        var linkedPorts = source!.LinkedPorts;
        Assert.That(linkedPorts.TryGetValue(sinkUid, out var links), Is.True);
        Assert.That(
            links!.Any(link => link.Source.ToString() == sourcePort && link.Sink.ToString() == sinkPort),
            Is.True);
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
            var coreDefinition = selection!.Definitions.Single(definition =>
                definition.PrefRoles.Any(role => role.ToString() == "MothershipCore"));
            var definition = selection.Definitions.Single(definition =>
                definition.PrefRoles.Any(role => role.ToString() == "Xenoborg"));
            Assert.Multiple(() =>
            {
                Assert.That(
                    coreDefinition.FallbackRoles.Select(role => role.ToString()),
                    Is.EqualTo(new[] { "Xenoborg" }));
                Assert.That(coreDefinition.SpawnerPrototype?.ToString(), Is.EqualTo("SpawnPointGhostRoleMothershipCore"));
                Assert.That(
                    definition.FallbackRoles.Select(role => role.ToString()),
                    Is.EqualTo(new[] { "MothershipCore" }));
                Assert.That(definition.SpawnerPrototype?.ToString(), Is.EqualTo("SpawnPointGhostRoleXenoborg"));
                Assert.That(definition.Min, Is.EqualTo(5));
                Assert.That(definition.Max, Is.EqualTo(5));
            });
        }

        Assert.That(protoMan.TryIndex<EntityPrototype>(RemovedPrintedMinerPrototype, out _), Is.False);
    }

    private static void AssertMinerPrototypes(IPrototypeManager protoMan, IComponentFactory componentFactory)
    {
        var core = protoMan.Index<EntityPrototype>(MothershipCorePrototype);
        Assert.That(
            core.TryGetComponent<PowerMonitoringCableNetworksComponent>(out _, componentFactory),
            Is.False);
        Assert.That(core.TryGetComponent<ActionGrantComponent>(out var actionGrant, componentFactory), Is.True);
        Assert.That(
            actionGrant!.Actions.Select(action => action.ToString()),
            Does.Contain("ActionMothershipEye"));
        Assert.That(protoMan.TryIndex<EntityPrototype>(MothershipEyePrototype, out _), Is.True);

        var standardGps = GetComponent<LavalandGpsTrackerComponent>(
            protoMan,
            componentFactory,
            "HandheldGPSBasic");
        var xenoborgGps = GetComponent<LavalandGpsTrackerComponent>(
            protoMan,
            componentFactory,
            "HandheldGPSXenoborg");
        Assert.Multiple(() =>
        {
            Assert.That(standardGps.Enabled, Is.True);
            Assert.That(xenoborgGps.Enabled, Is.False);
        });

        foreach (var moduleId in new[] { "XenoborgModuleBasic", "XenoborgModuleSpaceMovement" })
        {
            var hands = GetComponent<ItemBorgModuleComponent>(protoMan, componentFactory, moduleId)
                .Hands
                .Select(hand => hand.Item?.ToString());
            Assert.That(hands, Does.Contain("HandheldGPSXenoborg"));
            Assert.That(hands, Does.Not.Contain("HandheldGPSBasic"));
        }

        var miner = protoMan.Index<EntityPrototype>(MinerPrototype);
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

        var foreignModule = protoMan.Index<EntityPrototype>(HeavyLaserModulePrototype);
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
            Assert.That(
                protoMan.Index<EntityPrototype>(PortalGunPrototype)
                    .TryGetComponent<UseDelayOnShootComponent>(out _, componentFactory),
                Is.True);
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
        UseDelaySystem useDelay,
        EntityUid gridUid)
    {
        var grid = entMan.GetComponent<MapGridComponent>(gridUid);

        var gunUser = entMan.SpawnEntity(
            "XenoborgMiner",
            new EntityCoordinates(gridUid, XenoborgMarkers[0]));
        var cooldownGun = entMan.SpawnEntity(
            "WeaponXenoborgPortalGun",
            entMan.GetComponent<TransformComponent>(gunUser).Coordinates);
        var cooldownGunComponent = entMan.GetComponent<GunComponent>(cooldownGun);
        var firstAttempt = new ShotAttemptedEvent
        {
            User = gunUser,
            Used = (cooldownGun, cooldownGunComponent),
        };
        entMan.EventBus.RaiseLocalEvent(cooldownGun, ref firstAttempt);
        Assert.That(firstAttempt.Cancelled, Is.False);

        var shot = new GunShotEvent(gunUser, []);
        entMan.EventBus.RaiseLocalEvent(cooldownGun, ref shot);
        var repeatedAttempt = new ShotAttemptedEvent
        {
            User = gunUser,
            Used = (cooldownGun, cooldownGunComponent),
        };
        entMan.EventBus.RaiseLocalEvent(cooldownGun, ref repeatedAttempt);
        Assert.That(repeatedAttempt.Cancelled, Is.True);

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

        var goliath = entMan.SpawnEntity(
            "MobGoliath",
            new EntityCoordinates(gridUid, XenoborgMarkers[0]));
        var goliathOrigin = entMan.GetComponent<TransformComponent>(goliath).LocalPosition;
        Assert.That(FireAt(goliath).TotalSeconds, Is.EqualTo(120));
        var goliathXform = entMan.GetComponent<TransformComponent>(goliath);
        Assert.Multiple(() =>
        {
            Assert.That(goliathXform.GridUid, Is.EqualTo(gridUid));
            Assert.That(goliathXform.LocalPosition, Is.Not.EqualTo(goliathOrigin));
            Assert.That(turf.TryGetTileRef(goliathXform.Coordinates, out var tile), Is.True);
            Assert.That(turf.IsSpace(tile!.Value), Is.False);
            Assert.That(
                turf.IsTileBlocked(
                    tile.Value,
                    (CollisionGroup) entMan.GetComponent<PhysicsComponent>(goliath).CollisionMask),
                Is.False);
        });

        var liveTarget = entMan.SpawnEntity(
            "MobHuman",
            new EntityCoordinates(gridUid, XenoborgMarkers[0]));
        var safeDestinations = GetSafeDestinations(
            entMan,
            mapSystem,
            turf,
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

        var bossTarget = entMan.SpawnEntity(
            "MobGoliath",
            new EntityCoordinates(gridUid, XenoborgMarkers[3]));
        entMan.EnsureComponent<LavalandBossComponent>(bossTarget);
        var bossOrigin = entMan.GetComponent<TransformComponent>(bossTarget).Coordinates;
        Assert.That(FireAt(bossTarget).TotalSeconds, Is.EqualTo(3));
        Assert.That(
            entMan.GetComponent<TransformComponent>(bossTarget).Coordinates,
            Is.EqualTo(bossOrigin));
    }

    private static (EntityUid Core, EntityUid Eye) OpenMothershipEye(
        IEntityManager entMan,
        SharedPhysicsSystem physics,
        SharedTransformSystem transform,
        EntityUid gridUid)
    {
        var core = entMan.SpawnEntity("MothershipCore", new EntityCoordinates(gridUid, CoreMarker));
        var xenoborg = entMan.SpawnEntity("XenoborgEngi", new EntityCoordinates(gridUid, XenoborgMarkers[0]));
        Assert.Multiple(() =>
        {
            Assert.That(entMan.GetComponent<PhysicsComponent>(core).BodyType, Is.EqualTo(BodyType.Static));
            Assert.That(entMan.GetComponent<PhysicsComponent>(core).CanCollide, Is.True);
            Assert.That(physics.IsCurrentlyHardCollidable(core, xenoborg), Is.True);
            Assert.That(entMan.HasComponent<StationAiHeldComponent>(core), Is.True);
            Assert.That(entMan.HasComponent<StationAiOverlayComponent>(core), Is.False);
        });

        var open = new ToggleMothershipEyeEvent { Performer = core };
        entMan.EventBus.RaiseLocalEvent(core, open);
        Assert.That(open.Handled, Is.True);

        var eyes = new List<EntityUid>();
        var query = entMan.AllEntityQueryEnumerator<MothershipEyeComponent>();
        while (query.MoveNext(out var uid, out var eye))
        {
            if (eye.Core == core)
                eyes.Add(uid);
        }

        Assert.That(eyes, Has.Count.EqualTo(1));
        var eyeUid = eyes[0];
        Assert.Multiple(() =>
        {
            Assert.That(
                entMan.GetComponent<MetaDataComponent>(eyeUid).EntityPrototype?.ID,
                Is.EqualTo("XenoborgMothershipEye"));
            Assert.That(entMan.GetComponent<EyeComponent>(core).Target, Is.EqualTo(eyeUid));
            Assert.That(entMan.GetComponent<RelayInputMoverComponent>(core).RelayEntity, Is.EqualTo(eyeUid));
            Assert.That(entMan.GetComponent<InputMoverComponent>(core).CanMove, Is.False);
            Assert.That(entMan.GetComponent<InputMoverComponent>(eyeUid).CanMove, Is.True);
            Assert.That(entMan.GetComponent<PhysicsComponent>(core).CanCollide, Is.True);
            Assert.That(physics.IsCurrentlyHardCollidable(core, xenoborg), Is.True);
        });

        transform.SetCoordinates(eyeUid, new EntityCoordinates(gridUid, XenoborgMarkers[0]));
        Assert.That(
            entMan.GetComponent<TransformComponent>(eyeUid).LocalPosition,
            Is.EqualTo(XenoborgMarkers[0]));

        transform.SetCoordinates(eyeUid, new EntityCoordinates(gridUid, new Vector2(100.5f, 100.5f)));
        var restrictedXform = entMan.GetComponent<TransformComponent>(eyeUid);
        Assert.Multiple(() =>
        {
            Assert.That(restrictedXform.GridUid, Is.EqualTo(gridUid));
            Assert.That(restrictedXform.LocalPosition, Is.EqualTo(XenoborgMarkers[0]));
        });

        entMan.QueueDeleteEntity(xenoborg);
        return (core, eyeUid);
    }

    private static void CloseMothershipEye(IEntityManager entMan, EntityUid core, EntityUid eyeUid)
    {
        var close = new ToggleMothershipEyeEvent { Performer = core };
        entMan.EventBus.RaiseLocalEvent(core, close);
        Assert.Multiple(() =>
        {
            Assert.That(close.Handled, Is.True);
            Assert.That(entMan.GetComponent<EyeComponent>(core).Target, Is.Null);
            Assert.That(entMan.HasComponent<RelayInputMoverComponent>(core), Is.False);
            Assert.That(entMan.GetComponent<InputMoverComponent>(core).CanMove, Is.False);
            Assert.That(entMan.IsQueuedForDeletion(eyeUid), Is.True);
        });
    }

    private static HashSet<Vector2i> GetSafeDestinations(
        IEntityManager entMan,
        SharedMapSystem mapSystem,
        TurfSystem turf,
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
            $"unblocked={unblocked}, safe={safeTiles.Count}.");
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
        var user = entMan.SpawnEntity("XenoborgMiner", new EntityCoordinates(gridUid, XenoborgMarkers[3]));
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
        var bruteGroup = protoMan.Index<DamageGroupPrototype>(BruteDamageGroup);
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
