using Content.Shared.GameTicking;

namespace Content.Server.DeadSpace.Ports.Jukebox;

public sealed partial class ServerJukeboxSongsSyncSystem : EntitySystem
{
    [Dependency] private ServerJukeboxSongsSyncManager _jukeboxManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => _jukeboxManager?.CleanUp());
    }
}
