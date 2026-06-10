// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.DeadSpace.Necromorphs.Unitology.Components;
using Content.Shared.DeadSpace.Necromorphs.Sanity;
using Content.Shared.DeadSpace.Necromorphs.InfectionDead.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Zombies;
using Content.Shared.EntityEffects;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class CauseEnslavedUnitology : EntityEffect
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-cause-enslave", ("chance", Probability));

    public override void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? user)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        if (!entityManager.HasComponent<MobStateComponent>(target)
            || !entityManager.HasComponent<HumanoidAppearanceComponent>(target))
            return;

        if (entityManager.HasComponent<ImmunitetInfectionDeadComponent>(target))
        {
            DamageSpecifier dspec = new();
            dspec.DamageDict.Add("Cellular", 5f);
            entityManager.System<DamageableSystem>().TryChangeDamage(target, dspec, true, false);
            return;
        }

        if (entityManager.HasComponent<UnitologyComponent>(target)
            || entityManager.HasComponent<UnitologyEnslavedComponent>(target)
            || entityManager.HasComponent<NecromorfComponent>(target)
            || entityManager.HasComponent<ZombieComponent>(target)
            || !entityManager.HasComponent<SanityComponent>(target))
            return;

        entityManager.RemoveComponent<InfectionDeadComponent>(target);
        entityManager.EnsureComponent<UnitologyEnslavedComponent>(target);
    }
}