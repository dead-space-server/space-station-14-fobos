using Robust.Shared.Console;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Server;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Shared.GameTicking;
using Content.Shared.DeadSpace.Events;
using Robust.Shared.Timing;
using System.Threading;
using System.Linq;

using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.DeadSpace.ServerRestart;

public sealed class ServerRestartSystem : EntitySystem
{
    [Dependency] private readonly IBaseServer _server = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    private ISawmill _sawmill = default!;

    private bool _restartAfterRound;

    public override void Initialize()
    {
        _sawmill = _logManager.GetSawmill("restart");
        SubscribeNetworkEvent<ServerReloadRequestEvent>(HandleReloadRequest);
    }

    private void HandleReloadRequest(ServerReloadRequestEvent ev, EntitySessionEventArgs args)
    {
        if (!ev.ReloadNow)
        {
            _restartAfterRound = !_restartAfterRound;
            if (_restartAfterRound)
                _sawmill.Info("The server shutdown is scheduled after the end of the round.");
            else
                _sawmill.Info("Post-round shutdown cancelled");
        }
        else
        {
            DoShutdown();
        }
    }

    private void DoShutdown()
    {
        _sawmill.Debug($"Shutting down via {nameof(ServerRestartSystem)}!");
        _server.Shutdown(Loc.GetString("server-reload-shutdown"));
    }

    public void DoShutdownOnRoundEnd()
    {
        if (_restartAfterRound)
        {
            DoShutdown();
        }
    }
}
