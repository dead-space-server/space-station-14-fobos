// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class InfectiodDeadMutation : EntityEffectBase<InfectiodDeadMutation>
{
    [DataField]
    public float MutationStrength;

    [DataField]
    public bool IsStableMutation;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-mutate-infection-dead", ("chance", Probability));
}
