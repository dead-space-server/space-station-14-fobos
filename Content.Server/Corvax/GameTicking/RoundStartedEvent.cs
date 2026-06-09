namespace Content.Shared.GameTicking;

public sealed class RoundStartedEvent : EntityEventArgs
{
    public int RoundId { get; }
    public int PlayerCountAtStart { get; }
    public string? MapName { get; }
    
    public RoundStartedEvent(int roundId, int playerCountAtStart = 0, string? mapName = null)
    {
        RoundId = roundId;
        PlayerCountAtStart = playerCountAtStart;
        MapName = mapName;
    }
}
