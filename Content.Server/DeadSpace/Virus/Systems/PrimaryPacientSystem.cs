// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.Virus;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class PrimaryPacientSystem : EntitySystem
{
    [Dependency] private readonly SentientVirusSystem _sentientVirusSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrimaryPacientComponent, CureVirusEvent>(OnCureVirus);
        SubscribeLocalEvent<PrimaryPacientComponent, ComponentRemove>(OnRemove);
    }

    private void OnCureVirus(EntityUid uid, PrimaryPacientComponent component, CureVirusEvent args)
    {
        RemComp<PrimaryPacientComponent>(args.Target);
    }

    private void OnRemove(EntityUid uid, PrimaryPacientComponent component, ComponentRemove args)
    {
        if (component.SentientVirus != null
            && TryComp<SentientVirusComponent>(component.SentientVirus, out var sentientVirus))
            _sentientVirusSystem.RemovePrimaryInfected(component.SentientVirus.Value, uid, sentientVirus);
    }

}
