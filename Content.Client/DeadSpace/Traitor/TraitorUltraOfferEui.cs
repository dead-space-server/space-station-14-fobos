// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Eui;
using Content.Shared.DeadSpace.Traitor;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client.DeadSpace.Traitor;

[UsedImplicitly]
public sealed class TraitorUltraOfferEui : BaseEui
{
    private readonly TraitorUltraOfferWindow _window;
    private bool _sentResponse;

    public TraitorUltraOfferEui()
    {
        _window = new TraitorUltraOfferWindow();

        _window.AcceptButton.OnPressed += _ =>
        {
            _sentResponse = true;
            SendMessage(new TraitorUltraOfferChoiceMessage(TraitorUltraOfferButton.Accept));
            _window.Close();
        };

        _window.DeclineButton.OnPressed += _ =>
        {
            _sentResponse = true;
            SendMessage(new TraitorUltraOfferChoiceMessage(TraitorUltraOfferButton.Decline));
            _window.Close();
        };

        _window.OnClose += () =>
        {
            if (!_sentResponse)
                SendMessage(new TraitorUltraOfferChoiceMessage(TraitorUltraOfferButton.Decline));
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCenteredAt(new(0.5f, 0.45f));
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not TraitorUltraOfferEuiState offer)
            return;

        _window.SetState(offer.Title, offer.Body, offer.Gains, offer.Losses, offer.Accept, offer.Decline);
    }

    public override void Closed()
    {
        _window.Close();
    }
}
