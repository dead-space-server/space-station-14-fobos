// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Administration.Events;

[Serializable, NetSerializable]
public enum AutoMapVoteCategory
{
    Small,
    Medium,
    Large
}

[Serializable, NetSerializable]
public sealed class AutoMapVoteMapEntry
{
    public string Id = string.Empty;
    public string Name = string.Empty;
    public bool EligibleNow;
    public bool Blacklisted;
}

[Serializable, NetSerializable]
public sealed class AutoMapVoteCategoryStatus
{
    public AutoMapVoteCategory Category;
    public int MaxPlayers;
    public string MapsCsv = string.Empty;
    public int ConfiguredMapCount;
    public int ValidMapCount;
    public int EligibleMapCount;
    public string[] UnknownMaps = Array.Empty<string>();
}

[Serializable, NetSerializable]
public sealed class AutoMapVoteBlacklistStatus
{
    public string MapsCsv = string.Empty;
    public int ConfiguredMapCount;
    public int ValidMapCount;
    public string[] UnknownMaps = Array.Empty<string>();
}

[Serializable, NetSerializable]
public sealed class AutoMapVoteAdminState
{
    public bool Enabled;
    public bool VoteActive;
    public bool VoteBlocked;
    public int CurrentPlayerCount;
    public AutoMapVoteCategory CurrentCategory;
    public int VoteDurationSeconds;
    public AutoMapVoteMapEntry[] AvailableMaps = Array.Empty<AutoMapVoteMapEntry>();
    public AutoMapVoteCategoryStatus[] Categories = Array.Empty<AutoMapVoteCategoryStatus>();
    public AutoMapVoteBlacklistStatus Blacklist = new();
}

[Serializable, NetSerializable]
public sealed class AutoMapVoteAdminStateChangedEvent : EntityEventArgs
{
    public AutoMapVoteAdminState State;

    public AutoMapVoteAdminStateChangedEvent(AutoMapVoteAdminState state)
    {
        State = state;
    }
}
