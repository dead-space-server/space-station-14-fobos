// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Abilities;

public sealed partial class ThrowingThingsActionEvent : WorldTargetActionEvent
{
    [DataField]
    public int HowMuch = 3;

    [DataField]
    public float Range = 10f;

    [DataField]
    public float ThrowStrength = 10f;

    [DataField]
    public List<EntProtoId> Entities = new();

    [DataField]
    public List<string> Components = new();
}
