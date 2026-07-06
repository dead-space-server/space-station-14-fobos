using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Arena;

[Serializable, NetSerializable]
public sealed class ArenaJoinRequestEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class ArenaLeaveRequestEvent : EntityEventArgs;


