using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Skills.Events;

[Serializable, NetSerializable]
public sealed partial class LearnDoAfterEvent : SimpleDoAfterEvent
{ }
