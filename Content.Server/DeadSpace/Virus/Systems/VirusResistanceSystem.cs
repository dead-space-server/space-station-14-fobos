// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.Armor;
using Content.Shared.Inventory;
using Content.Shared.DeadSpace.Virus;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed partial class VirusResistanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusResistanceComponent, ArmorExamineEvent>(OnArmorExamine);
        SubscribeLocalEvent<VirusResistanceComponent, InventoryRelayedEvent<VirusResistanceQueryEvent>>(OnResistanceQuery);
    }

    private void OnResistanceQuery(Entity<VirusResistanceComponent> ent, ref InventoryRelayedEvent<VirusResistanceQueryEvent> query)
    {
        query.Args.TotalCoefficient *= ent.Comp.VirusResistanceCoefficient;
    }

    private void OnArmorExamine(Entity<VirusResistanceComponent> ent, ref ArmorExamineEvent args)
    {
        var value = MathF.Round(ent.Comp.VirusResistanceCoefficient * 100, 1);

        if (value == 0)
            return;

        args.Msg.PushNewline();
        args.Msg.AddMarkupOrThrow(Loc.GetString(ent.Comp.Examine, ("value", value)));
    }


}
