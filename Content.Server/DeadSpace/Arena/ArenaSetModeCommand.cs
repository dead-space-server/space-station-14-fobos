// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.DeadSpace.Arena;
using Robust.Shared.Console;

namespace Content.Server.DeadSpace.Arena;

[AdminCommand(AdminFlags.Fun)]
public sealed class ArenaSetModeCommand : LocalizedCommands
{
    public override string Command => "arenasetmode";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1)
        {
            shell.WriteLine("Usage: arenasetmode <deathmatch|tdm>");
            return;
        }
        var arenaSystem = EntitySystem.Get<ArenaSystem>();
        if (!arenaSystem.Enabled)
        {
            shell.WriteLine("Arena is disabled");
            return;
        }
        var modeStr = args[0].ToLowerInvariant();
        ArenaMode mode;
        switch (modeStr)
        {
            case "deathmatch":
                mode = ArenaMode.Deathmatch;
                break;
            case "tdm":
                mode = ArenaMode.TDM;
                break;
            default:
                shell.WriteLine("Unknown mode. Use: deathmatch, tdm");
                return;
        }
        arenaSystem.NextMode = mode;
        shell.WriteLine($"Next arena mode set to {mode}");
    }
}
