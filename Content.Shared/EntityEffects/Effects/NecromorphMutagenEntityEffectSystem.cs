// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Necromorphs.InfectionDead.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class NecromorphMutagenEntityEffectSystem : EntityEffectSystem<MobStateComponent, NecromorphMutagen>
{
    protected override void Effect(Entity<MobStateComponent> entity, ref EntityEffectEvent<NecromorphMutagen> args)
    {
        var target = entity.Owner;

        if (HasComp<HumanoidAppearanceComponent>(target) && args.Effect.IsAnimal)
            return;

        var component = EnsureComp<NecromorfAfterInfectionComponent>(target);
        component.NecroPrototype = args.Effect.NecroPrototype;
    }
}
