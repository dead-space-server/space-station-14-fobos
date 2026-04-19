// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.ERT.Components;
using Content.Shared.Implants;

namespace Content.Server.DeadSpace.ERT;

public sealed class ResponseErtImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResponseErtImplantComponent, ImplantImplantedEvent>(OnImplantImplanted);
    }

    private void OnImplantImplanted(Entity<ResponseErtImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (TryComp<ResponseErtOnAllowedStateComponent>(args.Implanted, out var imp))
        {
            imp.AllowedStates = ent.Comp.AllowedStates;
            imp.Team = ent.Comp.Team;
            imp.ActionPrototype = ent.Comp.ActionPrototype;
            imp.IsReady = true;
        }
        else
        {
            AddComp(args.Implanted, new ResponseErtOnAllowedStateComponent
            {
                AllowedStates = ent.Comp.AllowedStates,
                Team = ent.Comp.Team,
                ActionPrototype = ent.Comp.ActionPrototype,
                IsReady = true
            });
        }
    }
}
