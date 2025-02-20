using Content.Shared.DeadSpace.SmokingCarp.Abilities.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.DeadSpace.SmokingCarp.Abilities;

public abstract class SharedSmokingCarpNotRangeSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmokingCarpNotRangeComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShotAttempted(Entity<SmokingCarpNotRangeComponent> uid, ref ShotAttemptedEvent args)
    {
        Popup.PopupClient(Loc.GetString("gun-disabled"), uid, uid);
        args.Cancel();
    }
}
