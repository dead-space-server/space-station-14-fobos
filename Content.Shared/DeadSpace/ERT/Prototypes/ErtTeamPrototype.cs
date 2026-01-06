// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.Storage;

namespace Content.Shared.DeadSpace.ERT.Prototypes;

[Prototype("ertTeam")]
public sealed partial class ErtTeamPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; private set; } = string.Empty;

    [DataField]
    public string Description { get; private set; } = string.Empty;

    [DataField]
    public string? CancelMessage { get; private set; }

    [DataField("rule", required: true)]
    public EntProtoId ErtRule;

    [DataField("spawnWindow")]
    public TimedWindow TimeWindowToSpawn = new TimedWindow(TimeSpan.FromSeconds(600f), TimeSpan.FromSeconds(900f));

    [DataField]
    public int Price = 30000;

    /// <summary>
    ///     Особый Entity без которого не обойтись для спавна отряда.
    /// </summary>
    [DataField]
    public EntProtoId? Special = null;

    [DataField]
    public List<EntitySpawnEntry> Spawns = new();
}

