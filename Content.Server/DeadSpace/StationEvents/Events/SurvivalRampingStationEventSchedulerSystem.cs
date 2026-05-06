// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;

namespace Content.Server.StationEvents;

public sealed class SurvivalRampingStationEventSchedulerSystem : GameRuleSystem<SurvivalRampingStationEventSchedulerComponent>
{
    private const int EventPickAttempts = 20;

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public float GetChaosModifier(EntityUid uid, SurvivalRampingStationEventSchedulerComponent component)
    {
        var roundTime = (float) _gameTicker.RoundDuration().TotalSeconds;
        if (roundTime > component.EndTime)
            return component.MaxChaos;

        return component.MaxChaos / component.EndTime * roundTime + component.StartingChaos;
    }

    protected override void Started(EntityUid uid,
        SurvivalRampingStationEventSchedulerComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        component.MaxChaos = _random.NextFloat(component.AverageChaos - component.AverageChaos / 4, component.AverageChaos + component.AverageChaos / 4);
        component.EndTime = _random.NextFloat(component.AverageEndTime - component.AverageEndTime / 4, component.AverageEndTime + component.AverageEndTime / 4) * 60f;
        component.StartingChaos = component.MaxChaos / 10;

        PickNextEventTime(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_event.EventsEnabled)
            return;

        var query = EntityQueryEnumerator<SurvivalRampingStationEventSchedulerComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var scheduler, out var gameRule))
        {
            if (!GameTicker.IsGameRuleActive(uid, gameRule))
                continue;

            TryPlayAlert(scheduler);

            if (scheduler.TimeUntilNextEvent > 0f)
            {
                scheduler.TimeUntilNextEvent -= frameTime;
                continue;
            }

            var phase = PickPhase(scheduler);
            if (phase == null)
            {
                Log.Warning("Survival event scheduler has no active phase.");
                continue;
            }

            // Do not reset the cooldown until an event was actually queued.
            // GroupSelector may pick an empty sub-pool after CanRun filtering
            // e.g. heavy threats on low pop, so retry a few times before giving up
            // for this tick.
            if (!TryRunRandomEvent(phase))
                continue;

            PickNextEventTime(uid, scheduler);
        }
    }

    private void PickNextEventTime(EntityUid uid, SurvivalRampingStationEventSchedulerComponent component)
    {
        var mod = GetChaosModifier(uid, component);

        component.TimeUntilNextEvent = _random.NextFloat(240f / mod, 720f / mod);
    }

    private bool TryRunRandomEvent(SurvivalRampingStationEventSchedulerPhase phase)
    {
        for (var i = 0; i < EventPickAttempts; i++)
        {
            if (!_event.TryBuildLimitedEvents(phase.ScheduledGameRules, out var limitedEvents))
                continue;

            if (_event.FindEvent(limitedEvents) is not { } randomEvent)
                continue;

            _gameTicker.AddGameRule(randomEvent);
            return true;
        }

        return false;
    }


    private SurvivalRampingStationEventSchedulerPhase? PickPhase(SurvivalRampingStationEventSchedulerComponent component)
    {
        var roundMinutes = (float) _gameTicker.RoundDuration().TotalMinutes;
        SurvivalRampingStationEventSchedulerPhase? selected = null;

        foreach (var phase in component.Phases)
        {
            if (phase.StartTime > roundMinutes)
                continue;

            if (selected == null || phase.StartTime > selected.StartTime)
                selected = phase;
        }

        return selected;
    }

    private void TryPlayAlert(SurvivalRampingStationEventSchedulerComponent component)
    {
        if (component.AlertPlayed || component.AlertTime is not { } alertTime)
            return;

        if (_gameTicker.RoundDuration().TotalMinutes < alertTime)
            return;

        if (component.AlertAnnouncement is { } announcement)
        {
            _chat.DispatchGlobalAnnouncement(
                Loc.GetString(announcement),
                component.AlertSender is { } sender ? Loc.GetString(sender) : null,
                playSound: component.AlertSound != null,
                announcementSound: component.AlertSound,
                colorOverride: component.AlertAnnouncementColor);
        }

        component.AlertPlayed = true;
    }
}
