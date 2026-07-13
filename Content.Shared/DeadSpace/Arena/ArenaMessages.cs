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
    public int BlueKills { get; }
    public int RedKills { get; }

    public ArenaRoundUpdateEvent(ArenaMode mode, ArenaRoundState roundState, float timeRemaining, int blueKills = 0, int redKills = 0)
    {
        Mode = mode;
        RoundState = roundState;
        TimeRemaining = timeRemaining;
        BlueKills = blueKills;
        RedKills = redKills;
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

[Serializable, NetSerializable]
public sealed class ArenaPlayerRecord
{
    public string PlayerName = "";
    public int Kills;
    public int Deaths;
    public double KD;
    public int DmKills;
    public int DmDeaths;
    public int TdmKills;
    public int TdmDeaths;
}

[Serializable, NetSerializable]
public sealed class ArenaManifestEvent : EntityEventArgs
{
    public List<ArenaPlayerRecord> DmPlayers = new();
    public List<ArenaPlayerRecord> TdmPlayers = new();
    public ArenaTeam? BestTdmTeam;
    public ArenaPlayerRecord? OverallBest;
}
