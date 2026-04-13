// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.DeadSpace.Necromorphs.Unitology.Components;
using Content.Shared.DeadSpace.Necromorphs.Sanity;
using Content.Shared.DeadSpace.Necromorphs.InfectionDead.Components;
using Content.Shared.Damage;
using Content.Shared.Zombies;
using Content.Shared.Mindshield.Components;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class CauseEnslavedUnitology : EntityEffectBase<CauseEnslavedUnitology>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-cause-enslave", ("chance", Probability));
}
