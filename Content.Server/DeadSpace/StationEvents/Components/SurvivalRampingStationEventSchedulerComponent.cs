// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Audio;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(SurvivalRampingStationEventSchedulerSystem))]
public sealed partial class SurvivalRampingStationEventSchedulerComponent : Component
{
    /// <summary>
    ///     Average ending chaos modifier for the ramping event scheduler. Higher means faster.
    ///     Max chaos chosen for a round will deviate from this.
    /// </summary>
    [DataField]
    public float AverageChaos = 12f;

    /// <summary>
    ///     Average time (in minutes) for when the ramping event scheduler should stop increasing the chaos modifier.
    /// </summary>
    [DataField]
    public float AverageEndTime = 90f;

    [DataField]
    public float EndTime;

    [DataField]
    public float MaxChaos;

    [DataField]
    public float StartingChaos;

    [DataField]
    public float TimeUntilNextEvent;

    /// <summary>
    /// Survival-only event pools unlocked by round time.
    /// </summary>
    [DataField(required: true)]
    public List<SurvivalRampingStationEventSchedulerPhase> Phases = new();

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
