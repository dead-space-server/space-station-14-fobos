// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Administration.Events;

[Serializable, NetSerializable]
public sealed class RoundGameModeHistoryRequestEvent : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class RoundGameModeHistoryEntry
{
    public int RoundId;
    public int DayOffset;
    public string StartedAt = string.Empty;
    public string GameMode = string.Empty;
    public int PlayerCount = -1;
    public string MapName = string.Empty;
}

[Serializable, NetSerializable]
public sealed class RoundGameModeHistoryResponseEvent : EntityEventArgs
{
    public RoundGameModeHistoryEntry[] Entries = Array.Empty<RoundGameModeHistoryEntry>();
}
