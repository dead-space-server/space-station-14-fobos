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
}
