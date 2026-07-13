using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Arena;

[Serializable, NetSerializable]
public sealed class ArenaJoinEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class ArenaLeaveEvent : EntityEventArgs;

[Serializable, NetSerializable]
public enum ArenaMode : byte
{
    Deathmatch,
    PropHunt,
    TDM
}

[Serializable, NetSerializable]
public enum ArenaTeam : byte
{
    None,
    Blue,
    Red
}

[Serializable, NetSerializable]
public enum ArenaRoundState : byte
{
    Intermission,
    Hiding,
    Preparation,
    Active
}

[Serializable, NetSerializable]
public sealed class ArenaRoundUpdateEvent : EntityEventArgs
{
    public ArenaMode Mode { get; }
    public ArenaRoundState RoundState { get; }
    public float TimeRemaining { get; }

    public ArenaRoundUpdateEvent(ArenaMode mode, ArenaRoundState roundState, float timeRemaining)
    {
        Mode = mode;
        RoundState = roundState;
        TimeRemaining = timeRemaining;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaSeekerFreezeEvent : EntityEventArgs
{
    public float FreezeDuration { get; }

    public ArenaSeekerFreezeEvent(float freezeDuration)
    {
        FreezeDuration = freezeDuration;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaSeekerUnfreezeEvent : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class ArenaSeekerNotifyEvent : EntityEventArgs
{
    public string Message { get; }

    public ArenaSeekerNotifyEvent(string message)
    {
        Message = message;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaVoteCastEvent : EntityEventArgs
{
    public ArenaMode Vote { get; }

    public ArenaVoteCastEvent(ArenaMode vote)
    {
        Vote = vote;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaVoteStateEvent : EntityEventArgs
{
    public List<ArenaMode> AvailableModes { get; }
    public Dictionary<NetEntity, ArenaMode> Votes { get; }

    public ArenaVoteStateEvent(List<ArenaMode> availableModes, Dictionary<NetEntity, ArenaMode> votes)
    {
        AvailableModes = availableModes;
        Votes = votes;
    }
}
