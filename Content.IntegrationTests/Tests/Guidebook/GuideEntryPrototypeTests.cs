using Content.Client.Guidebook;
using Content.Client.Guidebook.Richtext;
using Robust.Shared.ContentPack;
using Robust.Shared.Log;
using Robust.Shared.Prototypes;
using System.Linq;
using Content.Shared.Guidebook;

namespace Content.IntegrationTests.Tests.Guidebook;

[TestFixture]
[TestOf(typeof(GuidebookSystem))]
[TestOf(typeof(GuideEntryPrototype))]
[TestOf(typeof(DocumentParsingManager))]
public sealed class GuideEntryPrototypeTests
{
    [Test]
    public async Task ValidatePrototypeContents()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var client = pair.Client;
        await client.WaitIdleAsync();
        var protoMan = client.ResolveDependency<IPrototypeManager>();
        var resMan = client.ResolveDependency<IResourceManager>();
        var parser = client.ResolveDependency<DocumentParsingManager>();
        var prototypes = protoMan.EnumeratePrototypes<GuideEntryPrototype>().ToList();

        // Suppress "Hit style update limit" warnings that occur when parsing large guidebook pages
        var logMan = client.ResolveDependency<ILogManager>();
        var uiSawmill = logMan.GetSawmill("ui");
        var oldLevel = uiSawmill.Level;
        uiSawmill.Level = LogLevel.Error;

        foreach (var proto in prototypes)
        {
            await client.WaitAssertion(() =>
            {
                using var reader = resMan.ContentFileReadText(proto.Text);
                var text = reader.ReadToEnd();
                Assert.That(parser.TryAddMarkup(new Document(), text), $"Failed to parse guidebook: {proto.Id}");
            });

            // Avoid style update limit
            await client.WaitRunTicks(1);
        }

        uiSawmill.Level = oldLevel;
        await pair.CleanReturnAsync();
    }
}
