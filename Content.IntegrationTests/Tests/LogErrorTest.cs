using Robust.Shared.Configuration;
using Robust.Shared.Log;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests;

public sealed class LogErrorTest
{
    /// <summary>
    ///     This test ensures that error logs cause tests to fail.
    /// </summary>
    [Test]
    public async Task TestLogErrorCausesTestFailure()
    {
        await AssertLogErrorCausesTestFailure(true);
        await AssertLogErrorCausesTestFailure(false);
    }

    private static async Task AssertLogErrorCausesTestFailure(bool serverSide)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var server = pair.Server;
        var client = pair.Client;

        IIntegrationInstance instance = serverSide ? server : client;
        var failingLogs = serverSide
            ? pair.ServerLogHandler.FailingLogs
            : pair.ClientLogHandler.FailingLogs;
        var expectedPrefix = serverSide ? "SERVER" : "CLIENT";

        await instance.WaitPost(() => instance.CfgMan.SetCVar(RTCVars.FailureLogLevel, LogLevel.Error));
        Assert.That(instance.CfgMan.GetCVar(RTCVars.FailureLogLevel), Is.EqualTo(LogLevel.Error));

        // Warnings don't cause tests to fail.
        await instance.WaitPost(() => instance.Log.Warning("test"));
        Assert.That(failingLogs, Is.Empty);

        // But errors do. Robust may throw immediately when the log is written, but the
        // pooled test pair must also report the stored failing log when returned.
        await instance.WaitPost(() =>
        {
            try
            {
                instance.Log.Error("test");
            }
            catch (AssertionException)
            {
                // Expected on some Robust versions.
            }
        });

        Assert.That(failingLogs, Has.Count.EqualTo(1));
        Assert.That(failingLogs[0], Does.Contain(expectedPrefix).And.Contain("test"));

        Assert.That(
            async () => await pair.CleanReturnAsync(),
            Throws.Exception.With.Message.Contains(expectedPrefix).And.Message.Contains("test"));
    }
}
