using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

public sealed partial class ShadowlingVeilActionEvent : InstantActionEvent {}
public sealed partial class ShadowlingPhaseActionEvent : InstantActionEvent {}

[Serializable, NetSerializable]
public sealed partial class ShadowlingAscendanceDoAfterEvent : SimpleDoAfterEvent {}
