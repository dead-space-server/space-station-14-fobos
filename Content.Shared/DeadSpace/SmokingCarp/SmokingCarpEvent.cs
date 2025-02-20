using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.SmokingCarp;

public sealed partial class PowerPunchCarpEvent : InstantActionEvent { }
public sealed partial class SmokePunchCarpEvent : InstantActionEvent { }
public sealed partial class TripPunchCarpEvent : InstantActionEvent { }
public sealed partial class ReflectCarpEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed partial class GiveSmokingCarpDoAfterEvent : SimpleDoAfterEvent { }
