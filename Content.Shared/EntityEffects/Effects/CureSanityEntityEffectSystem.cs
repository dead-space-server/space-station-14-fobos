// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Necromorphs.Sanity;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class CureSanityEntityEffectSystem : EntityEffectSystem<SanityComponent, CureSanity>
{
    protected override void Effect(Entity<SanityComponent> entity, ref EntityEffectEvent<CureSanity> args)
    {
        EntityManager.System<SharedSanitySystem>().TryAddSanityLvl(entity, 100);
    }
}
