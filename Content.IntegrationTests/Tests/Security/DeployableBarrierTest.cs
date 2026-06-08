using Content.Shared.Lock;
using Content.Shared.Security.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.IntegrationTests.Tests.Security;

[TestFixture]
public sealed class DeployableBarrierTest
{
    [Test]
    public async Task UndeployedBarrierDoesNotCollide()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entManager = server.ResolveDependency<IEntityManager>();
        var lockSystem = entManager.System<LockSystem>();

        var testMap = await pair.CreateTestMap();
        EntityUid barrier = default;

        await server.WaitPost(() =>
        {
            barrier = entManager.SpawnEntity("DeployableBarrier", testMap.GridCoords);
        });

        await server.WaitAssertion(() => AssertBarrierState(entManager, barrier, false));

        await server.WaitPost(() => lockSystem.Lock(barrier, null));
        await server.WaitAssertion(() => AssertBarrierState(entManager, barrier, true));

        await server.WaitPost(() => lockSystem.Unlock(barrier, null));
        await server.WaitAssertion(() => AssertBarrierState(entManager, barrier, false));

        await pair.CleanReturnAsync();
    }

    private static void AssertBarrierState(IEntityManager entManager, EntityUid uid, bool deployed)
    {
        var transform = entManager.GetComponent<TransformComponent>(uid);
        var physics = entManager.GetComponent<PhysicsComponent>(uid);
        var fixtures = entManager.GetComponent<FixturesComponent>(uid);
        var barrier = entManager.GetComponent<DeployableBarrierComponent>(uid);

        Assert.Multiple(() =>
        {
            Assert.That(transform.Anchored, Is.EqualTo(deployed));
            Assert.That(physics.CanCollide, Is.EqualTo(deployed));
            Assert.That(fixtures.Fixtures[barrier.FixtureId].Hard, Is.EqualTo(deployed));
        });
    }
}
