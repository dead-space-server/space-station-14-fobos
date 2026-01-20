using Robust.Shared.Console;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Server;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Shared.GameTicking;

namespace Content.Server.DeadSpace.ServerReload;

public sealed class ServerRestartSystem : EntitySystem
{
    [Dependency] private readonly IBaseServer _server = default!;
    [Dependency] private readonly IConsoleHost _consoleHost = default!;

    private ISawmill _sawmill = default!;

    private bool _restartAfterRound;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);

        _consoleHost.RegisterCommand("reload", "Server shutdown with reload reason", "test1", DoShutdownCommand);
        _consoleHost.RegisterCommand("reload_after_round", "Server shutdown after round end", "test2", ReloadAfterRoundCommand);

    }

    private void DoShutdownCommand(IConsoleShell shell, string argStr, string[] args)
    {
        DoShutdown();
    }

    private void DoShutdown()
    {
        _sawmill.Debug($"Shutting down via {nameof(ServerRestartSystem)}!");
        _server.Shutdown(Loc.GetString("server-reload-shutdown"));
    }

    private void ReloadAfterRoundCommand(IConsoleShell shell, string argStr, string[] args)
    {
        if (_restartAfterRound)
        {
            _restartAfterRound = false;
        }
        else
        {
            _restartAfterRound = true;
        }
    }


    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        if (_restartAfterRound)
        {
            _restartAfterRound = false;
            DoShutdown();
        }
    }
}
