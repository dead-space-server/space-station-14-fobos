using Content.Server.DeadSpace.Lavaland.Components;
using Content.Server.Stack;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Content.Shared.Storage;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.DeadSpace.Lavaland;

[TestFixture]
public sealed class LavalandOreRedeemerTest
{
    private const string SteelOre = "SteelOre";
    private const string GoldOre = "GoldOre";

    [Test]
    public async Task FullStorageInsertRedeemsInsertedOre()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = false,
            Dirty = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var storage = server.System<StorageSystem>();
        var stack = server.System<StackSystem>();

        EntityUid incomingOre = default;

        await server.WaitPost(() =>
        {
            var oreBox = entMan.SpawnEntity("OreBox", MapCoordinates.Nullspace);
            var oreBag = entMan.SpawnEntity("OreBag", MapCoordinates.Nullspace);
            incomingOre = SpawnOre(entMan, stack, SteelOre, 30);

            Assert.That(storage.Insert(oreBag, incomingOre, out _, playSound: false), Is.True);

            var ev = RedeemBagIntoBox(entMan, oreBag, oreBox);

            Assert.That(ev.Handled, Is.True);
        });

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(entMan.GetComponent<StackComponent>(incomingOre).Count, Is.EqualTo(30));
                Assert.That(entMan.GetComponent<LavalandRedeemedOreComponent>(incomingOre).ProcessedUnits, Is.EqualTo(30));
                Assert.That(entMan.GetComponent<LavalandRedeemedOreComponent>(incomingOre).CreditedUnits, Is.EqualTo(0));
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task PartialStorageMergeDoesNotRedeemLeftoverOre()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = false,
            Dirty = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var storage = server.System<StorageSystem>();
        var stack = server.System<StackSystem>();

        EntityUid existingOre = default;
        EntityUid incomingOre = default;

        await server.WaitPost(() =>
        {
            var oreBox = entMan.SpawnEntity("OreBox", MapCoordinates.Nullspace);
            var oreBag = entMan.SpawnEntity("OreBag", MapCoordinates.Nullspace);
            existingOre = SpawnOre(entMan, stack, SteelOre, 20);
            incomingOre = SpawnOre(entMan, stack, SteelOre, 30);

            Assert.That(storage.Insert(oreBox, existingOre, out _, playSound: false), Is.True);
            Assert.That(storage.Insert(oreBag, incomingOre, out _, playSound: false), Is.True);

            BlockNewStorageItems(entMan, oreBox);

            var ev = RedeemBagIntoBox(entMan, oreBag, oreBox);

            Assert.That(ev.Handled, Is.True);
        });

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(entMan.GetComponent<StackComponent>(existingOre).Count, Is.EqualTo(30));
                Assert.That(entMan.GetComponent<StackComponent>(incomingOre).Count, Is.EqualTo(20));
                Assert.That(entMan.GetComponent<LavalandRedeemedOreComponent>(existingOre).ProcessedUnits, Is.EqualTo(10));
                Assert.That(entMan.GetComponent<LavalandRedeemedOreComponent>(existingOre).CreditedUnits, Is.EqualTo(0));
                AssertUnredeemed(entMan, incomingOre);
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FullStorageDoesNotRedeemUninsertedOre()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = false,
            Dirty = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var storage = server.System<StorageSystem>();
        var stack = server.System<StackSystem>();

        EntityUid existingOre = default;
        EntityUid incomingOre = default;

        await server.WaitPost(() =>
        {
            var oreBox = entMan.SpawnEntity("OreBox", MapCoordinates.Nullspace);
            var oreBag = entMan.SpawnEntity("OreBag", MapCoordinates.Nullspace);
            existingOre = SpawnOre(entMan, stack, SteelOre, 30);
            incomingOre = SpawnOre(entMan, stack, SteelOre, 30);

            Assert.That(storage.Insert(oreBox, existingOre, out _, playSound: false), Is.True);
            Assert.That(storage.Insert(oreBag, incomingOre, out _, playSound: false), Is.True);

            BlockNewStorageItems(entMan, oreBox);

            var ev = RedeemBagIntoBox(entMan, oreBag, oreBox);

            Assert.That(ev.Handled, Is.True);
        });

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(entMan.GetComponent<StackComponent>(existingOre).Count, Is.EqualTo(30));
                Assert.That(entMan.GetComponent<StackComponent>(incomingOre).Count, Is.EqualTo(30));
                AssertUnredeemed(entMan, existingOre);
                AssertUnredeemed(entMan, incomingOre);
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task DifferentOreWithoutSlotDoesNotRedeemUninsertedOre()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = false,
            Dirty = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var storage = server.System<StorageSystem>();
        var stack = server.System<StackSystem>();

        EntityUid existingOre = default;
        EntityUid incomingOre = default;

        await server.WaitPost(() =>
        {
            var oreBox = entMan.SpawnEntity("OreBox", MapCoordinates.Nullspace);
            var oreBag = entMan.SpawnEntity("OreBag", MapCoordinates.Nullspace);
            existingOre = SpawnOre(entMan, stack, SteelOre, 20);
            incomingOre = SpawnOre(entMan, stack, GoldOre, 30);

            Assert.That(storage.Insert(oreBox, existingOre, out _, playSound: false), Is.True);
            Assert.That(storage.Insert(oreBag, incomingOre, out _, playSound: false), Is.True);

            BlockNewStorageItems(entMan, oreBox);

            var ev = RedeemBagIntoBox(entMan, oreBag, oreBox);

            Assert.That(ev.Handled, Is.True);
        });

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(entMan.GetComponent<StackComponent>(existingOre).Count, Is.EqualTo(20));
                Assert.That(entMan.GetComponent<StackComponent>(incomingOre).Count, Is.EqualTo(30));
                AssertUnredeemed(entMan, existingOre);
                AssertUnredeemed(entMan, incomingOre);
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task PartialStorageMergeKeepsRedeemedStateOnInsertedUnitsOnly()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = false,
            Dirty = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var storage = server.System<StorageSystem>();
        var stack = server.System<StackSystem>();

        EntityUid existingOre = default;
        EntityUid incomingOre = default;

        await server.WaitPost(() =>
        {
            var oreBox = entMan.SpawnEntity("OreBox", MapCoordinates.Nullspace);
            var oreBag = entMan.SpawnEntity("OreBag", MapCoordinates.Nullspace);
            existingOre = SpawnOre(entMan, stack, SteelOre, 15);
            incomingOre = SpawnOre(entMan, stack, SteelOre, 30);

            var redeemed = entMan.EnsureComponent<LavalandRedeemedOreComponent>(incomingOre);
            redeemed.ProcessedUnits = 10;
            redeemed.CreditedUnits = 10;

            Assert.That(storage.Insert(oreBox, existingOre, out _, playSound: false), Is.True);
            Assert.That(storage.Insert(oreBag, incomingOre, out _, playSound: false), Is.True);

            BlockNewStorageItems(entMan, oreBox);

            var ev = RedeemBagIntoBox(entMan, oreBag, oreBox);

            Assert.That(ev.Handled, Is.True);
        });

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(entMan.GetComponent<StackComponent>(existingOre).Count, Is.EqualTo(30));
                Assert.That(entMan.GetComponent<StackComponent>(incomingOre).Count, Is.EqualTo(15));
                Assert.That(entMan.GetComponent<LavalandRedeemedOreComponent>(existingOre).ProcessedUnits, Is.EqualTo(15));
                Assert.That(entMan.GetComponent<LavalandRedeemedOreComponent>(existingOre).CreditedUnits, Is.EqualTo(10));
                AssertUnredeemed(entMan, incomingOre);
            });
        });

        await pair.CleanReturnAsync();
    }

    private static EntityUid SpawnOre(IEntityManager entMan, StackSystem stack, string prototype, int count)
    {
        var ore = entMan.SpawnEntity(prototype, MapCoordinates.Nullspace);
        stack.SetCount((ore, entMan.GetComponent<StackComponent>(ore)), count);
        return ore;
    }

    private static void AssertUnredeemed(IEntityManager entMan, EntityUid ore)
    {
        if (!entMan.TryGetComponent<LavalandRedeemedOreComponent>(ore, out var redeemed))
            return;

        Assert.Multiple(() =>
        {
            Assert.That(redeemed.ProcessedUnits, Is.EqualTo(0));
            Assert.That(redeemed.CreditedUnits, Is.EqualTo(0));
        });
    }

    private static void BlockNewStorageItems(IEntityManager entMan, EntityUid storageUid)
    {
        var storage = entMan.GetComponent<StorageComponent>(storageUid);
        storage.Grid.Clear();
        storage.OccupiedGrid.Clear();
    }

    private static InteractUsingEvent RedeemBagIntoBox(IEntityManager entMan, EntityUid oreBag, EntityUid oreBox)
    {
        var coordinates = entMan.GetComponent<TransformComponent>(oreBox).Coordinates;
        var ev = new InteractUsingEvent(oreBag, oreBag, oreBox, coordinates);
        entMan.EventBus.RaiseLocalEvent(oreBox, ev);
        return ev;
    }
}
