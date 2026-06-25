// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Chemistry.Events;
using Content.Shared.DeadSpace.Chemistry.Components;

namespace Content.Shared.DeadSpace.Chemistry;

public sealed class InjectionBlockerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InjectionBlockerComponent, TargetBeforeInjectEvent>(OnBeforeInject);
    }

    private void OnBeforeInject(Entity<InjectionBlockerComponent> ent, ref TargetBeforeInjectEvent args)
    {
        if (ent.Comp.BlockedMessage is { } message)
            args.OverrideMessage = Loc.GetString(message, ("target", ent.Owner), ("user", args.EntityUsingInjector), ("injector", args.UsedInjector));

        args.Cancel();
    }
}
