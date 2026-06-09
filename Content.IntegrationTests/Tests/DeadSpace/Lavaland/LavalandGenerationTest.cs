// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Lavaland.Components;
using Content.Server.GameTicking;
using Content.Shared.CCVar;
using Content.Shared.DeadSpace.CCCCVars;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.IntegrationTests.Tests.DeadSpace.Lavaland;

[TestFixture]
[NonParallelizable]
public sealed class LavalandGenerationTest
{
    [Test]
    public async Task AutoGenerateCompletesAndRoundRestartDeletesGeneratedMap()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = false,
            Dirty = true,
        });

        var server = pair.Server;
        var cfg = server.ResolveDependency<IConfigurationManager>();
        var entMan = server.EntMan;
        var ticker = server.System<GameTicker>();

        await server.WaitPost(() =>
        {
            cfg.SetCVar(CCCCVars.LavalandAutoGenerate, true);
            cfg.SetCVar(CCVars.GameDummyTicker, false);
            cfg.SetCVar(CCVars.GameMap, PoolManager.TestMap);
            ticker.RestartRound();
        });

        EntityUid? generatedMap = null;
        await PoolManager.WaitUntil(server, () =>
        {
            var query = entMan.EntityQueryEnumerator<StationLavalandComponent>();
            while (query.MoveNext(out _, out var lavaland))
            {
                if (lavaland.GeneratedMap is not { Valid: true } map ||
                    entMan.Deleted(map) ||
                    !entMan.EntityExists(map) ||
                    lavaland.GenerationTask != null ||
                    lavaland.GenerationCancel != null)
                {
                    continue;
                }

                generatedMap = map;
                return true;
            }

            return false;
        }, maxTicks: 3600, tickStep: 5);

        Assert.That(generatedMap, Is.Not.Null);
        var generatedMapUid = generatedMap.Value;
        Assert.That(entMan.HasComponent<MapComponent>(generatedMapUid), Is.True);

        await server.WaitPost(() =>
        {
            cfg.SetCVar(CCCCVars.LavalandAutoGenerate, false);
            ticker.RestartRound();
        });

        await PoolManager.WaitUntil(server, () =>
            !entMan.EntityExists(generatedMapUid) || entMan.Deleted(generatedMapUid));

        await pair.CleanReturnAsync();
    }
}
