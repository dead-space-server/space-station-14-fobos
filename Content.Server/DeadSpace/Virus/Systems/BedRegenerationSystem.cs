// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.Virus;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class BedRegenerationSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BedRegenerationComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<BedRegenerationComponent, UnstrappedEvent>(OnUnstrapped);
    }

    private void OnStrapped(Entity<BedRegenerationComponent> bed, ref StrappedEvent args)
    {
        if (TryComp<VirusComponent>(args.Buckle, out var virusComponent))
            virusComponent.RegenerationType = bed.Comp.RegenerationType;
    }

    private void OnUnstrapped(Entity<BedRegenerationComponent> bed, ref UnstrappedEvent args)
    {
        if (TryComp<VirusComponent>(args.Buckle, out var virusComponent))
            virusComponent.RegenerationType = BedRegenerationType.None;
    }
}
