// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.EUI;
using Content.Shared.DeadSpace.Traitor;
using Content.Shared.Eui;

namespace Content.Server.DeadSpace.Traitor;

public sealed class TraitorUltraExtraObjectiveOfferEui : BaseEui
{
    private readonly EntityUid _rule;
    private readonly EntityUid _mindId;
    private readonly TraitorUltraRuleSystem _system;

    public TraitorUltraExtraObjectiveOfferEui(EntityUid rule, EntityUid mindId, TraitorUltraRuleSystem system)
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
        return _system.GetExtraObjectiveOfferState(_rule, _mindId);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (IsShutDown)
            return;

        if (msg is not TraitorUltraExtraObjectiveOfferChoiceMessage choice)
            return;

        _system.HandleExtraObjectiveOffer(
            _rule,
            _mindId,
            choice.Button == TraitorUltraExtraObjectiveOfferButton.Accept);

        if (!IsShutDown)
            Close();
    }

    public override void Closed()
    {
        _system.OnExtraObjectiveOfferEuiClosed(_mindId, this);
    }
}
