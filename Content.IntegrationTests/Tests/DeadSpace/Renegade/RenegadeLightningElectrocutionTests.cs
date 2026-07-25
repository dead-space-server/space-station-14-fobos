// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Electrocution;
using Content.Shared.Damage.Components;
using Content.Shared.Electrocution;
using Content.Shared.FixedPoint;
using Content.Shared.Stunnable;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.DeadSpace.Renegade;

[TestFixture]
[NonParallelizable]
public sealed class RenegadeLightningElectrocutionTests
{
    private const string UnprotectedPrototype = "RenegadeLightningTestUnprotected";
    private const string ProtectedPrototype = "RenegadeLightningTestProtected";

    [TestPrototypes]
    private const string Prototypes = $"""
        - type: entity
          id: {UnprotectedPrototype}
          parent: MobHuman
          components:
          - type: Insulated
            lightningProtectionChance: 0

        - type: entity
          id: {ProtectedPrototype}
          parent: MobHuman
          components:
          - type: Insulated
            lightningProtectionChance: 1
        """;

    [Test]
    public async Task LightningCanStunWithoutDamageAndUsesProtectionChance()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        await server.WaitAssertion(() =>
        {
            var entMan = server.EntMan;
            var electrocution = server.System<ElectrocutionSystem>();
            var unprotected = entMan.SpawnEntity(UnprotectedPrototype, MapCoordinates.Nullspace);
            var protectedTarget = entMan.SpawnEntity(ProtectedPrototype, MapCoordinates.Nullspace);

            var stunned = electrocution.TryDoElectrocution(
                unprotected,
                sourceUid: null,
                shockDamage: null,
                time: TimeSpan.FromSeconds(5),
                refresh: true,
                isLightning: true);
            var protectedByInsulation = !electrocution.TryDoElectrocution(
                protectedTarget,
                sourceUid: null,
                shockDamage: null,
                time: TimeSpan.FromSeconds(5),
                refresh: true,
                isLightning: true);

            Assert.Multiple(() =>
            {
                Assert.That(stunned, Is.True);
                Assert.That(entMan.HasComponent<ElectrocutedComponent>(unprotected), Is.True);
                Assert.That(entMan.HasComponent<StunnedComponent>(unprotected), Is.True);
                Assert.That(entMan.GetComponent<DamageableComponent>(unprotected).TotalDamage,
                    Is.EqualTo(FixedPoint2.Zero));

                Assert.That(protectedByInsulation, Is.True);
                Assert.That(entMan.HasComponent<ElectrocutedComponent>(protectedTarget), Is.False);
                Assert.That(entMan.HasComponent<StunnedComponent>(protectedTarget), Is.False);
                Assert.That(entMan.GetComponent<DamageableComponent>(protectedTarget).TotalDamage,
                    Is.EqualTo(FixedPoint2.Zero));
            });
        });

        await pair.CleanReturnAsync();
    }
}
