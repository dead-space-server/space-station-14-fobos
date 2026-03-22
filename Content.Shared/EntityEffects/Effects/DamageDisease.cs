// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class DamageDisease : EntityEffectBase<DamageDisease>
{
    [DataField]
    public float BaseDamage = 1f;
    [DataField]
    public float ResistanceIncrease = 0.05f;

    /// <summary>
    /// The reagent prototype ID used for per-medicine resistance tracking.
    /// Should match the reagent this effect is defined on.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype>? Medicine;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-damage-disease", ("chance", Probability));
}
