// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Administration;
using Content.Shared.DeadSpace.StationAI.UI;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;

namespace Content.Client.DeadSpace.StationAI;

[AnyCommand]
public sealed class StationAiTrackEntityCommand : IConsoleCommand
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;

    public string Command => "ai_track_entity";
    public string Description => Loc.GetString("cmd-ai-track-entity-desc");
    public string Help => Loc.GetString("cmd-ai-track-entity-help", ("command", Command));

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !NetEntity.TryParse(args[0], out var target))
        {
            shell.WriteLine(Help);
            return;
        }

        _entitySystem.GetEntitySystem<StationAiTrackEntitySystem>().Track(target);
    }
}

public sealed class StationAiTrackEntitySystem : EntitySystem
{
    public void Track(NetEntity target)
    {
        RaiseNetworkEvent(new StationAiTrackEntityNetworkEvent(target));
    }
}
