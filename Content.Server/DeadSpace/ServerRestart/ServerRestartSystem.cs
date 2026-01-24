using Robust.Server;
using Content.Server.GameTicking;
using Content.Shared.DeadSpace.Events;
using Robust.Shared.Timing;
using Content.Server.Chat.Managers;

using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.DeadSpace.ServerRestart;

public sealed class ServerRestartSystem : EntitySystem
{
    [Dependency] private readonly IBaseServer _server = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
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
            {
                _sawmill.Info("The server shutdown is scheduled after the end of the round.");
                _chatManager.DispatchServerAnnouncement(Loc.GetString("server-reload-scheduled"));
            }
            else
            {
                _sawmill.Info("Post-round shutdown cancelled");
                _chatManager.DispatchServerAnnouncement(Loc.GetString("server-reload-canceled"));
            }
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

    public bool RoundEnded()
    {
        if (_restartAfterRound)
        {
            DoShutdown();
            return true;
        }

        return false;
    }
}
