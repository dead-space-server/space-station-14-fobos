// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Audio;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(SurvivalRampingStationEventSchedulerSystem))]
public sealed partial class SurvivalRampingStationEventSchedulerComponent : Component
{
    /// <summary>
    ///     Ending chaos modifier for the ramping event scheduler. Higher means faster.
    /// </summary>
    [DataField]
    public float AverageChaos = 2.4f;

    /// <summary>
    ///     Time (in minutes) for when the ramping event scheduler should stop increasing the chaos modifier.
    /// </summary>
    [DataField]
    public float AverageEndTime = 45f;

    /// <summary>
    ///     Time (in minutes) when the ramping event scheduler should finish falling back to the initial chaos modifier.
    /// </summary>
    [DataField]
    public float MinimumChaosTime = 70f;

    [DataField]
    public float EndTime;

    [DataField]
    public float FalloffEndTime;

    [DataField]
    public float MaxChaos;

    /// <summary>
    ///     Initial chaos modifier for the ramping event scheduler.
    /// </summary>
    [DataField]
    public float StartingChaos = 1f;

    [DataField]
    public float TimeUntilNextEvent;

    /// <summary>
    /// Survival-only event pools unlocked by round time.
    /// </summary>
    [DataField(required: true)]
    public List<SurvivalRampingStationEventSchedulerPhase> Phases = new();

    /// <summary>
    /// Survival-only per-event occurrence caps. These are combined with StationEvent maxOccurrences,
    /// and the lower cap wins.
    /// </summary>
    [DataField]
    public Dictionary<string, int> MaxEventOccurrences = new();

    [DataField]
    public float? AlertTime;

    [DataField]
    public LocId? AlertAnnouncement;

    [DataField]
    public LocId? AlertSender;

    [DataField]
    public Color AlertAnnouncementColor = Color.Gold;

    [DataField]
    public SoundSpecifier? AlertSound;

    [DataField]
    public bool AlertPlayed;
}

[DataDefinition]
public sealed partial class SurvivalRampingStationEventSchedulerPhase
{
    /// <summary>
    /// Round time in minutes when this phase becomes active.
    /// </summary>
    [DataField(required: true)]
    public float StartTime;

    /// <summary>
    /// The gamerules that the scheduler can choose from for this phase.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector ScheduledGameRules = default!;
}
