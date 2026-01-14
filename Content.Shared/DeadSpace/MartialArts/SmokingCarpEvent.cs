using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.MartialArts.SmokingCarp;

public sealed partial class SmokingCarpPowerPunchEvent : InstantActionEvent { }
public sealed partial class SmokingCarpSmokePunchEvent : IntantActionEvent { }
public sealed partial class ReflectCarpEvent : InstantActionEvent { }
public sealed partial class SmokingCarpTripPunchEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed class SmokingCarpSaying(LocId saying) : EntityEventArgs
{
    public LocId Saying = saying;
};
