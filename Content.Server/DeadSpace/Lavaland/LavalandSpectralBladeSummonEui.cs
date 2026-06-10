using Content.Server.EUI;
using Content.Shared.DeadSpace.Lavaland;
using Content.Shared.Eui;

namespace Content.Server.DeadSpace.Lavaland;

public sealed class LavalandSpectralBladeSummonEui : BaseEui
{
    private readonly EntityUid _blade;
    private readonly EntityUid _summoner;
    private readonly LavalandSpectralBladeSystem _system;
    private readonly TimeSpan _expiresAt;
    private readonly string _summonerName;
    private bool _handled;

    public LavalandSpectralBladeSummonEui(
        EntityUid blade,
        EntityUid summoner,
        LavalandSpectralBladeSystem system,
        TimeSpan expiresAt,
        string summonerName)
    {
        _blade = blade;
        _summoner = summoner;
        _system = system;
        _expiresAt = expiresAt;
        _summonerName = summonerName;
    }

    public override void Opened()
    {
        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        return new LavalandSpectralBladeSummonEuiState(_summonerName);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (_handled)
            return;

        _handled = true;

        if (msg is LavalandSpectralBladeSummonResponseMessage response)
            _system.HandleSpectralBladeSummonResponse(_blade, _summoner, Player, response.Accepted, _expiresAt);

        Close();
    }
}
