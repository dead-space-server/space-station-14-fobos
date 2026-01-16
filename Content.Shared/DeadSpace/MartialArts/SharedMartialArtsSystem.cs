using Content.Shared.DeadSpace.MartialArts.SmokingCarp.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.DeadSpace.MartialArts.SmokingCarp;

public abstract class SharedMartialArtsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SmokingCarpNotShotComponent, ShotAttemptedEvent>(OnShotAttempt);
    }

    private void OnShotAttempt(Entity<SmokingCarpNotShotComponent> ent, ref ShotAttemptedEvent args)
    {
        _popup.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }
}
