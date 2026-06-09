using Content.Client.Eui;
using Content.Shared.DeadSpace.Lavaland;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client.DeadSpace.Lavaland;

[UsedImplicitly]
public sealed class LavalandSpectralBladeSummonEui : BaseEui
{
    private readonly YesNoWindow _window;
    private bool _sent;

    public LavalandSpectralBladeSummonEui()
    {
        _window = new YesNoWindow(
            Loc.GetString("lavaland-spectral-blade-summon-title"),
            Loc.GetString("lavaland-spectral-blade-summon-message", ("name", string.Empty)));

        _window.YesButton.Text = Loc.GetString("lavaland-spectral-blade-summon-yes");
        _window.NoButton.Text = Loc.GetString("lavaland-spectral-blade-summon-no");

        _window.YesButton.OnPressed += _ => SendResponse(true);
        _window.NoButton.OnPressed += _ => SendResponse(false);
        _window.OnClose += () => SendResponse(false);
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not LavalandSpectralBladeSummonEuiState summonState)
            return;

        _window.Title = Loc.GetString("lavaland-spectral-blade-summon-title");
        _window.MessageLabel.Text = Loc.GetString("lavaland-spectral-blade-summon-message", ("name", summonState.SummonerName));
    }

    public override void Closed()
    {
        _sent = true;
        _window.Close();
    }

    private void SendResponse(bool accepted)
    {
        if (_sent)
            return;

        _sent = true;
        SendMessage(new LavalandSpectralBladeSummonResponseMessage(accepted));
        _window.Close();
    }
}
