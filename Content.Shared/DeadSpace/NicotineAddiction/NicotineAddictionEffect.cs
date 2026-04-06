using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.DeadSpace.NicotineAddiction;

public sealed partial class NicotineAddictionEffect : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<NicotineAddictionComponent>(args.TargetEntity, out var comp))
            return;

        var timing = IoCManager.Resolve<IGameTiming>();
        comp.LastNicotineInBloodTime = timing.CurTime;
        comp.DeprivationPopupShown = false;
        comp.DeprivationPopupShownAt = TimeSpan.Zero;
        if (!comp.DeprivationShakeActive)
            return;

        comp.DeprivationShakeActive = false;
        args.EntityManager.Dirty(args.TargetEntity, comp);
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return null;
    }
}
