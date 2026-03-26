// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Server.DeadSpace.Virus.Systems;
using Content.Shared.Body.Components;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Content.Shared.Mobs.Components;

namespace Content.Server.EntityEffects.Effects.DeadSpace;

/// <summary>
/// Causes a virus infection on this entity.
/// Attempts to find VirusData from the entity's bloodstream chemical solution.
/// </summary>
public sealed partial class CauseVirusEntityEffectSystem : EntityEffectSystem<MobStateComponent, CauseVirus>
{
    [Dependency] private readonly VirusSystem _virus = default!;

    protected override void Effect(Entity<MobStateComponent> entity, ref EntityEffectEvent<CauseVirus> args)
    {
        VirusData? data = null;

        // Try to find VirusData from the entity's bloodstream solution.
        // The virus data is carried as ReagentData on reagent instances in the solution.
        if (TryComp<BloodstreamComponent>(entity, out var bloodstream)
            && bloodstream.BloodSolution is { } bloodSolutionEntity)
        {
            foreach (var reagent in bloodSolutionEntity.Comp.Solution.Contents)
            {
                var dataList = reagent.Reagent.Data;
                if (dataList == null)
                    continue;

                data = dataList.OfType<VirusData>().FirstOrDefault();
                if (data != null)
                    break;
            }
        }

        if (data == null)
            return;

        _virus.ProbInfect(data, entity);
    }
}
