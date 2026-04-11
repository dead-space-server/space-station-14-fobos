using Content.Shared.Administration;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Server.Administration;
using Robust.Shared.Console;

namespace Content.Server.DeadSpace.Demons.Shadowling;

[AdminCommand(AdminFlags.Fun)]
public sealed class MakeShadowlingCommand : IConsoleCommand
{
    public string Command => "makeshadowling";
    public string Description => "Превращает цель в скрытого тенеморфа.";
    public string Help => "makeshadowling ";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {

        var entityManager = IoCManager.Resolve<IEntityManager>();

        if (!NetEntity.TryParse(args[0], out var netEntity) ||
            !entityManager.TryGetEntity(netEntity, out var target))
        {
            shell.WriteError("Неверный EntityUid.");
            return;
        }

        if (entityManager.HasComponent<ShadowlingRevealComponent>(target.Value))
            return;
    }
}
