using System.Linq;
using Content.Server.Pinpointer;
using Content.Shared.Pinpointer;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Pinpointer;

[TestFixture]
public sealed class NavMapBeaconTest
{
    private const string BeaconText = "Custom Beacon";

    [TestPrototypes]
    private const string Prototypes = $@"
- type: entity
  id: TestNavMapBeaconWithText
  components:
  - type: Transform
    anchored: true
  - type: Appearance
  - type: NavMapBeacon
    text: {BeaconText}
";

    [Test]
    public async Task BeaconWithPresetTextRegistersAndUpdates()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = true,
            DummyTicker = false
        });

        var server = pair.Server;
        var map = await pair.CreateTestMap();
        await pair.RunTicksSync(5);

        var entMan = server.ResolveDependency<IEntityManager>();
        var mapSystem = server.System<SharedMapSystem>();
        var navMapSystem = server.System<NavMapSystem>();

        EntityUid beacon = default;
        NavMapComponent navMap = default!;

        await server.WaitAssertion(() =>
        {
            navMap = entMan.EnsureComponent<NavMapComponent>(map.Grid);
            beacon = entMan.SpawnEntity("TestNavMapBeaconWithText", map.GridCoords);
        });

        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            Assert.That(navMap.Beacons.Values.Single().Text, Is.EqualTo(BeaconText));
        });

        await server.WaitAssertion(() =>
        {
            navMapSystem.SetBeaconEnabled(beacon, false);
            Assert.That(navMap.Beacons, Is.Empty);

            navMapSystem.SetBeaconEnabled(beacon, true);
            Assert.That(navMap.Beacons.Values.Single().Text, Is.EqualTo(BeaconText));
        });

        await server.WaitPost(() => mapSystem.DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }
}
