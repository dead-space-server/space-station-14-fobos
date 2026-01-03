// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Popups;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Virus;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class PrimaryPacientSystem : EntitySystem
{
    [Dependency] private readonly SentientVirusSystem _sentientVirusSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly VirusSystem _virus = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    private const int Compensation = 5000;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrimaryPacientComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PrimaryPacientComponent, CureVirusEvent>(OnCureVirus);
        SubscribeLocalEvent<PrimaryPacientComponent, ComponentRemove>(OnRemove);

        SubscribeLocalEvent<PrimaryPacientComponent, EnterCryostorageEvent>(OnMindRemoved);
    }

    private void OnMindRemoved(EntityUid uid, PrimaryPacientComponent component, EnterCryostorageEvent args)
    {
        if (!TryComp<SentientVirusComponent>(component.SentientVirus, out var sentientVirusComp))
            return;

        if (sentientVirusComp.Data != null)
        {
            sentientVirusComp.Data.MutationPoints += Compensation;
            sentientVirusComp.FactPrimaryInfected--;
            _popupSystem.PopupEntity(
                Loc.GetString("sentient-virus-infect-compensation", ("price", Compensation)),
                component.SentientVirus.Value,
                component.SentientVirus.Value,
                PopupType.Medium
            );
        }

        _virus.CureVirus(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PrimaryPacientComponent, VirusComponent>();
        while (query.MoveNext(out var uid, out var component, out var virusComponent))
        {
            if (component.UpdateWindow != null && component.UpdateWindow.IsExpired())
            {
                component.UpdateWindow.Reset();
                _virus.InfectAround((uid, virusComponent), component.RangeInfect);
            }
        }
    }

    private void OnInit(EntityUid uid, PrimaryPacientComponent component, ComponentInit args)
    {
        component.UpdateWindow = new TimedWindow(component.MinUpdateDuration, component.MaxUpdateDuration, _timing, _random);
    }

    private void OnCureVirus(EntityUid uid, PrimaryPacientComponent component, CureVirusEvent args)
    {
        RemComp<PrimaryPacientComponent>(uid);
    }

    private void OnRemove(EntityUid uid, PrimaryPacientComponent component, ComponentRemove args)
    {
        if (component.SentientVirus != null
            && TryComp<SentientVirusComponent>(component.SentientVirus, out var sentientVirus))
            _sentientVirusSystem.RemovePrimaryInfected(component.SentientVirus.Value, uid, sentientVirus);
    }

}
