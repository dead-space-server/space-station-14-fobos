using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.NicotineAddiction;

public sealed partial class NicotineAddictionEffect : EntityEffectBase<NicotineAddictionEffect>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return null;
    }
}
