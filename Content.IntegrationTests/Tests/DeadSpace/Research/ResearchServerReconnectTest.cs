// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Power.Components;
using Content.Server.Research.Systems;
using Content.Shared.Power;
using Content.Shared.Research.Components;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.DeadSpace.Research;

[TestFixture]
[TestOf(typeof(ResearchSystem))]
public sealed class ResearchServerReconnectTest
{
    [TestPrototypes]
    private const string Prototypes = @"
- type: entity
  id: ResearchReconnectServerDummy
  components:
  - type: Transform
    anchored: true
  - type: ResearchServer
  - type: ApcPowerReceiver

- type: entity
  id: ResearchReconnectClientDummy
  components:
  - type: Transform
    anchored: true
  - type: ResearchClient
  - type: UserInterface
    interfaces:
      enum.ResearchClientUiKey.Key:
        type: ResearchClientBoundUserInterface
";

    [Test]
    public async Task ClientsReconnectToAnAvailableServerAfterPowerLoss()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.EntMan;

        await server.WaitAssertion(() =>
        {
            var firstServer = entMan.SpawnEntity("ResearchReconnectServerDummy", testMap.GridCoords);
            var secondServer = entMan.SpawnEntity("ResearchReconnectServerDummy", testMap.GridCoords);
            var client = entMan.SpawnEntity("ResearchReconnectClientDummy", testMap.GridCoords);

            var firstPower = entMan.GetComponent<ApcPowerReceiverComponent>(firstServer);
            var secondPower = entMan.GetComponent<ApcPowerReceiverComponent>(secondServer);
            var clientComponent = entMan.GetComponent<ResearchClientComponent>(client);
            var firstServerComponent = entMan.GetComponent<ResearchServerComponent>(firstServer);
            var secondServerComponent = entMan.GetComponent<ResearchServerComponent>(secondServer);

            Assert.That(clientComponent.Server, Is.Null);

            SetPowered(entMan, firstServer, firstPower, true);
            Assert.Multiple(() =>
            {
                Assert.That(clientComponent.Server, Is.EqualTo(firstServer));
                Assert.That(firstServerComponent.Clients, Does.Contain(client));
            });

            SetPowered(entMan, secondServer, secondPower, true);
            Assert.That(clientComponent.Server, Is.EqualTo(firstServer),
                "An available server must not replace the client's existing selection.");

            SetPowered(entMan, firstServer, firstPower, false);
            Assert.Multiple(() =>
            {
                Assert.That(clientComponent.Server, Is.EqualTo(secondServer));
                Assert.That(firstServerComponent.Clients, Does.Not.Contain(client));
                Assert.That(secondServerComponent.Clients, Does.Contain(client));
            });

            SetPowered(entMan, secondServer, secondPower, false);
            Assert.That(clientComponent.Server, Is.Null);

            SetPowered(entMan, firstServer, firstPower, true);
            Assert.Multiple(() =>
            {
                Assert.That(clientComponent.Server, Is.EqualTo(firstServer));
                Assert.That(firstServerComponent.Clients, Does.Contain(client));
            });
        });

        await pair.CleanReturnAsync();
    }

    private static void SetPowered(
        IEntityManager entMan,
        EntityUid server,
        ApcPowerReceiverComponent power,
        bool powered)
    {
        power.Powered = powered;
        var powerChanged = new PowerChangedEvent(powered, 0f);
        entMan.EventBus.RaiseLocalEvent(server, ref powerChanged);
    }
}
