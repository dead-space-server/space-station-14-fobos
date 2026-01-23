// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.ERT.Components;
using Content.Shared.Implants;

namespace Content.Server.DeadSpace.ERT;

public sealed class ResponceErtImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResponceErtImplantComponent, ImplantImplantedEvent>(OnImplantImplanted);
    }

    private void OnImplantImplanted(Entity<ResponceErtImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (TryComp<ResponceErtOnAllowedStateComponent>(args.Implanted, out var imp))
        {
            imp.AllowedStates = ent.Comp.AllowedStates;
            imp.Team = ent.Comp.Team;
            imp.ActionPrototype = ent.Comp.ActionPrototype;
            imp.IsReady = true;
        }
        else
        {
            AddComp(args.Implanted, new ResponceErtOnAllowedStateComponent
            {
                AllowedStates = ent.Comp.AllowedStates,
                Team = ent.Comp.Team,
                ActionPrototype = ent.Comp.ActionPrototype,
                IsReady = true
            });
        }
    }
}
