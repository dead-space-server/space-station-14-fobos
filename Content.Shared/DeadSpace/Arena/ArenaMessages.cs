using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Arena;

[Serializable, NetSerializable]
public sealed class ArenaJoinEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class ArenaLeaveEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class ArenaPickEvent : EntityEventArgs
{
    public int Pick { get; }

    public ArenaPickEvent(int pick)
    {
        Pick = pick;
    }
}
