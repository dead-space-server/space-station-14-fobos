using Robust.Shared.Maths;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Arena;

public static class ArenaConstants
{
    /// <summary>лок рас.</summary>
    public static readonly IReadOnlySet<string> SpeciesBlacklist = new HashSet<string> { "IPC", "Vox" };

    /// <summary>Валюта. Пока не нужна, не настроено сохранение</summary>
    public const int KillCurrencyReward = 1;

    /// <summary>Цвета команд для окрашивания снаряжения в режиме TDM.</summary>
    public static readonly Color TdmTeamBlueColor = new(0.25f, 0.55f, 1.0f);
    public static readonly Color TdmTeamRedColor = new(1.0f, 0.25f, 0.25f);

    /// <summary>
    /// Возвращает цвет команды для тонировки, либо null если команда не задана.
    /// </summary>
    public static Color? GetTeamColor(ArenaTeam team)
    {
        return team switch
        {
            ArenaTeam.Blue => TdmTeamBlueColor,
            ArenaTeam.Red => TdmTeamRedColor,
            _ => null,
        };
    }
}

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
public sealed class ArenaPickEvent : EntityEventArgs
{
    public int Pick { get; }

    public ArenaPickEvent(int pick)
    {
        Pick = pick;
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

/// <summary>
/// Итоги арены за раунд. Рассылается сервером при окончании раунда.
/// </summary>
[Serializable, NetSerializable]
public sealed class ArenaManifestEvent : EntityEventArgs
{
    /// <summary>Игроки арены (K/D за раунд).</summary>
    public List<ArenaPlayerRecord> Players = new();

    /// <summary>Лучшие игроки Deathmatch за время арены.</summary>
    public List<ArenaPlayerRecord> DmPlayers = new();

    /// <summary>Лучшие игроки TDM за время арены.</summary>
    public List<ArenaPlayerRecord> TdmPlayers = new();

    /// <summary>Лучшая команда TDM.</summary>
    public ArenaTeam? BestTdmTeam;

    /// <summary>Счёт команд TDM за время арены.</summary>
    public int BlueScore;

    /// <summary>Счёт команд TDM за время арены.</summary>
    public int RedScore;

    /// <summary>Лучший игрок арены по сумме DM + TDM.</summary>
    public ArenaPlayerRecord? OverallBest;
}
