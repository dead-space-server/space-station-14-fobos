// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class DamageDisease : EventEntityEffect<DamageDisease>
{
    [DataField]
    public float BaseDamage = 1f;
    [DataField]
    public float ResistanceIncrease = 0.05f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-damage-disease", ("chance", Probability));
}
