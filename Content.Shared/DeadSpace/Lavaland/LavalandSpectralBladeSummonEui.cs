using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Lavaland;

[Serializable, NetSerializable]
public sealed class LavalandSpectralBladeSummonEuiState : EuiStateBase
{
    public readonly string SummonerName;

    public LavalandSpectralBladeSummonEuiState(string summonerName)
    {
        SummonerName = summonerName;
    }
}

[Serializable, NetSerializable]
public sealed class LavalandSpectralBladeSummonResponseMessage : EuiMessageBase
{
    public readonly bool Accepted;

    public LavalandSpectralBladeSummonResponseMessage(bool accepted)
    {
        Accepted = accepted;
    }
}
