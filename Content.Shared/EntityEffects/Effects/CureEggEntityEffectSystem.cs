// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Abilities.Egg.Components;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class CureEggEntityEffectSystem : EntityEffectSystem<EggComponent, CureEgg>
{
    protected override void Effect(Entity<EggComponent> entity, ref EntityEffectEvent<CureEgg> args)
    {
        RemComp<EggComponent>(entity);
    }
}
