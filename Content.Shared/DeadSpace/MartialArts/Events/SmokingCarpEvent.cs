// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
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