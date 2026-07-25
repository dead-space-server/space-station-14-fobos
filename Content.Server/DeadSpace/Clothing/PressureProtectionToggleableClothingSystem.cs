// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Atmos.Components;
using Content.Shared.Clothing.EntitySystems;

namespace Content.Server.DeadSpace.Clothing;

/// <summary>
/// Prevents pressure-protective headwear from being hidden under an attached hardsuit helmet.
/// </summary>
public sealed class PressureProtectionToggleableClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PressureProtectionComponent, ToggleableClothingStorageAttemptEvent>(OnStorageAttempt);
    }

    private void OnStorageAttempt(Entity<PressureProtectionComponent> ent,
        ref ToggleableClothingStorageAttemptEvent args)
    {
        args.Cancel();
    }
}
