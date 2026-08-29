// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Atmos;
using Content.Shared.StatusEffectNew;

namespace Content.Server.DeadSpace.Atmos.Halon;

public sealed class HalonProtectionSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<HalonProtectionComponent, GetFireProtectionEvent>(OnGetFireProtection);
        SubscribeLocalEvent<HalonProtectionStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusApplied);
        SubscribeLocalEvent<HalonProtectionStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusRemoved);
    }

    private void OnGetFireProtection(Entity<HalonProtectionComponent> ent, ref GetFireProtectionEvent args)
    {
        args.Reduce(1f);
    }

    private void OnStatusApplied(Entity<HalonProtectionStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        EnsureComp<HalonProtectionComponent>(args.Target);
    }

    private void OnStatusRemoved(Entity<HalonProtectionStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        RemComp<HalonProtectionComponent>(args.Target);
    }
}
