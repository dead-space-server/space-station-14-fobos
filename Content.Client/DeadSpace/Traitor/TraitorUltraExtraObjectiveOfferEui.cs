// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Eui;
using Content.Shared.DeadSpace.Traitor;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client.DeadSpace.Traitor;

[UsedImplicitly]
public sealed class TraitorUltraExtraObjectiveOfferEui : BaseEui
{
    private readonly TraitorUltraExtraObjectiveOfferWindow _window;

    public TraitorUltraExtraObjectiveOfferEui()
    {
        _window = new TraitorUltraExtraObjectiveOfferWindow();

        _window.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new TraitorUltraExtraObjectiveOfferChoiceMessage(TraitorUltraExtraObjectiveOfferButton.Accept));
            _window.Close();
        };

        _window.DeclineButton.OnPressed += _ =>
        {
            SendMessage(new TraitorUltraExtraObjectiveOfferChoiceMessage(TraitorUltraExtraObjectiveOfferButton.Decline));
            _window.Close();
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCenteredAt(new(0.5f, 0.45f));
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not TraitorUltraExtraObjectiveOfferEuiState offer)
            return;

        _window.SetState(offer.Title, offer.Body, offer.ObjectiveName, offer.Reward, offer.Accept, offer.Decline);
    }

    public override void Closed()
    {
        _window.Close();
    }
}
