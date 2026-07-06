using Content.Server.Communications;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed class LockShuttleCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly CommunicationsConsoleSystem _comms = default!;

        public override string Command => "lockshuttle";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            _comms.ToggleLockEvac();
        }
    }
}