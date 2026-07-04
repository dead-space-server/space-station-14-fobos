using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DeadSpace.FlipTable;

[Serializable, NetSerializable]
public sealed partial class FlipTableDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class UnflipTableDoAfterEvent : SimpleDoAfterEvent
{
}
