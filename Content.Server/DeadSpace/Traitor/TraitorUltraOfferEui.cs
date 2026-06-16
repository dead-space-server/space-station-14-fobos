// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.EUI;
using Content.Shared.DeadSpace.Traitor;
using Content.Shared.Eui;

namespace Content.Server.DeadSpace.Traitor;

public sealed class TraitorUltraOfferEui : BaseEui
{
    private readonly EntityUid _rule;
    private readonly EntityUid _mindId;
    private readonly TraitorUltraRuleSystem _system;

    public TraitorUltraOfferEui(EntityUid rule, EntityUid mindId, TraitorUltraRuleSystem system)
    {
        _rule = rule;
        _mindId = mindId;
        _system = system;
    }

    public override void Opened()
    {
        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        return _system.GetOfferState(_rule, _mindId);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not TraitorUltraOfferChoiceMessage choice ||
            choice.Button == TraitorUltraOfferButton.Decline)
        {
            _system.HandleUpgradeOffer(_rule, _mindId, false);
            Close();
            return;
        }

        _system.HandleUpgradeOffer(_rule, _mindId, true);
        Close();
    }

    public override void Closed()
    {
        _system.HandleUpgradeOffer(_rule, _mindId, false);
    }
}
