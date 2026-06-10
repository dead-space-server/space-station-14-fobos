// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.Storage;
using Content.Shared.Body.Components;
using Content.Shared.DeadSpace.Abilities.Egg;
using Content.Shared.Mobs.Systems;
using Content.Shared.DeadSpace.Abilities.Egg.Components;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class CauseEgg : EntityEffectBase<CauseEgg>
{
    [DataField("spawned", required: true)]
    public List<EntitySpawnEntry> SpawnedEntities = new();

    [DataField]
    public float Duration = 60f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-cause-egg", ("chance", Probability));
}
