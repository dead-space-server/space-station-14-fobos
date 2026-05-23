// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.RoundEnd;
using Content.Server.StationEvents.Components;
using Content.Shared.EntityTable;
using Content.Shared.GameTicking.Components;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.StationEvents;

public sealed class SurvivalRampingStationEventSchedulerSystem : GameRuleSystem<SurvivalRampingStationEventSchedulerComponent>
{
    private const int EventPickAttempts = 20;

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;

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
            // GroupSelector may pick a sub-pool that has no queueable events after
            // max occurrence / active-rule filtering, so retry a few times.
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
            if (!TryBuildSurvivalEvents(phase, out var limitedEvents))
                continue;

            if (_event.FindEvent(limitedEvents) is not { } randomEvent)
                continue;

            _gameTicker.AddGameRule(randomEvent);
            return true;
        }

        return false;
    }

    private bool TryBuildSurvivalEvents(
        SurvivalRampingStationEventSchedulerPhase phase,
        out Dictionary<EntityPrototype, StationEventComponent> limitedEvents)
    {
        limitedEvents = new Dictionary<EntityPrototype, StationEventComponent>();

        foreach (var eventId in _entityTable.GetSpawns(phase.ScheduledGameRules))
        {
            if (!_prototype.Resolve(eventId, out var eventPrototype))
            {
                Log.Warning($"Survival event ID {eventId} has no prototype index.");
                continue;
            }

            if (limitedEvents.ContainsKey(eventPrototype))
                continue;

            if (eventPrototype.Abstract)
                continue;

            if (!eventPrototype.TryGetComponent<StationEventComponent>(out var stationEvent, EntityManager.ComponentFactory))
                continue;

            if (!CanQueueSurvivalEvent(eventPrototype, stationEvent))
                continue;

            limitedEvents.Add(eventPrototype, stationEvent);
        }

        return limitedEvents.Count > 0;
    }

    private bool CanQueueSurvivalEvent(EntityPrototype prototype, StationEventComponent stationEvent)
    {
        // Survival phase tables are the local timing gate, so ignore only the
        // StationEvent time gates: EarliestStart and ReoccurrenceDelay.
        if (_gameTicker.IsGameRuleActive(prototype.ID))
            return false;

        if (stationEvent.WillNotStartRandomly)
            return false;

        if (stationEvent.MaxOccurrences.HasValue && GetOccurrences(prototype.ID) >= stationEvent.MaxOccurrences.Value)
            return false;

        if (_player.PlayerCount < stationEvent.MinimumPlayers)
            return false;

        if (_roundEnd.IsRoundEndRequested() && !stationEvent.OccursDuringRoundEnd)
            return false;

        return true;
    }

    private int GetOccurrences(string stationEvent)
    {
        return _gameTicker.AllPreviousGameRules.Count(rule => rule.Item2 == stationEvent);
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
