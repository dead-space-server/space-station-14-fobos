// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.EUI;
using Content.Shared.DeadSpace.Traitor;
using Content.Shared.Eui;

namespace Content.Server.DeadSpace.Traitor;

public sealed class TraitorUltraRecruitEui : BaseEui
{
    private readonly EntityUid _rule;
    private readonly EntityUid _mindId;
    private readonly TraitorUltraRuleSystem _system;
    private readonly string? _corporation;

    public TraitorUltraRecruitEui(EntityUid rule, EntityUid mindId, TraitorUltraRuleSystem system, string? corporation)
    {
        _rule = rule;
        _mindId = mindId;
        _system = system;
        _corporation = corporation;
    }

    public override void Opened()
    {
        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        return new TraitorUltraRecruitEuiState(
            Loc.GetString("traitor-ultra-recruit-title"),
            Loc.GetString(
                "traitor-ultra-recruit-body",
                ("corp", string.IsNullOrWhiteSpace(_corporation)
                    ? Loc.GetString("objective-issuer-unknown")
                    : Loc.GetString(_corporation))),
            Loc.GetString("traitor-ultra-recruit-accept"),
            Loc.GetString("traitor-ultra-recruit-decline"));
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not TraitorUltraRecruitChoiceMessage choice)
        {
            Close();
            return;
        }

        _system.HandleRecruitOffer(_rule, _mindId, choice.Button == TraitorUltraRecruitButton.Accept);
        Close();
    }

    public override void Closed()
    {
        _system.HandleRecruitOffer(_rule, _mindId, false);
    }
}
