using Content.Shared.FixedPoint;
using Content.Shared.Mobs;

namespace Content.Shared.Traits.Assorted;

[RegisterComponent]
public sealed partial class PainNumbnessMobThresholdComponent : Component
{
    public SortedDictionary<FixedPoint2, MobState> OriginalThresholds = new();
}
