// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Eui;
using Content.Shared.DeadSpace.Traitor;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client.DeadSpace.Traitor;

[UsedImplicitly]
public sealed class TraitorUltraRecruitEui : BaseEui
{
    private readonly TraitorUltraRecruitWindow _window;
    private bool _sentResponse;

    public TraitorUltraRecruitEui()
    {
        _window = new TraitorUltraRecruitWindow();

        _window.AcceptButton.OnPressed += _ =>
        {
            _sentResponse = true;
            SendMessage(new TraitorUltraRecruitChoiceMessage(TraitorUltraRecruitButton.Accept));
            _window.Close();
        };

        _window.DeclineButton.OnPressed += _ =>
        {
            _sentResponse = true;
            SendMessage(new TraitorUltraRecruitChoiceMessage(TraitorUltraRecruitButton.Decline));
            _window.Close();
        };

        _window.OnClose += () =>
        {
            if (!_sentResponse)
                SendMessage(new TraitorUltraRecruitChoiceMessage(TraitorUltraRecruitButton.Decline));
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not TraitorUltraRecruitEuiState recruit)
            return;

        _window.SetState(recruit.Title, recruit.Body, recruit.Accept, recruit.Decline);
    }

    public override void Closed()
    {
        _window.Close();
    }
}
