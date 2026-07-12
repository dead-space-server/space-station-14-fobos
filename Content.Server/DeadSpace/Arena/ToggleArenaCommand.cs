// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.DeadSpace.Arena;

[AdminCommand(AdminFlags.Fun)]
public sealed class ToggleArenaCommand : LocalizedCommands
{
    public override string Command => "togglearena";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var arenaSystem = EntitySystem.Get<ArenaSystem>();
        arenaSystem.ToggleEnabled();
        shell.WriteLine($"Arena is now {(arenaSystem.Enabled ? "enabled" : "disabled")}");
    }
}
