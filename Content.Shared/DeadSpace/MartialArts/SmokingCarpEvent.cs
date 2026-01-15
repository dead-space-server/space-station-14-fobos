using Content.Shared.Actions;
using Content.Shared.DeadSpace.MartialArts.SmokingCarp.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.MartialArts.SmokingCarp;

public sealed partial class SmokingCarpPowerPunchEvent : InstantActionEvent { }
public sealed partial class SmokingCarpSmokePunchEvent : InstantActionEvent { }
public sealed partial class ReflectCarpEvent : InstantActionEvent { }
public sealed partial class SmokingCarpTripPunchEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed class SmokingCarpSaying(LocId saying) : EntityEventArgs
{
    public LocId Saying = saying;
};

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
