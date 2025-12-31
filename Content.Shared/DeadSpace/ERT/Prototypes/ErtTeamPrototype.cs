// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.TimeWindow;

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

    [DataField("rule", required: true)]
    public EntProtoId ErtRule;

    [DataField("spawnWindow")]
    public TimedWindow TimeWindowToSpawn = new TimedWindow(600f, 900f);

    [DataField]
    public int Min = 30000;

    [DataField]
    public float Max = 30000;

    [DataField]
    public int Price = 30000;
}

