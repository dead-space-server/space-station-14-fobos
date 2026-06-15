// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Traitor;

[Serializable, NetSerializable]
public enum TraitorUltraOfferButton : byte
{
    Decline,
    Accept,
}

[Serializable, NetSerializable]
public sealed class TraitorUltraOfferChoiceMessage : EuiMessageBase
{
    public readonly TraitorUltraOfferButton Button;

    public TraitorUltraOfferChoiceMessage(TraitorUltraOfferButton button)
    {
        Button = button;
    }
}

[Serializable, NetSerializable]
public sealed class TraitorUltraOfferEuiState : EuiStateBase
{
    public string Title = string.Empty;
    public string Body = string.Empty;
    public string Gains = string.Empty;
    public string Losses = string.Empty;
    public string Accept = string.Empty;
    public string Decline = string.Empty;

    public TraitorUltraOfferEuiState(
        string title,
        string body,
        string gains,
        string losses,
        string accept,
        string decline)
    {
        Title = title;
        Body = body;
        Gains = gains;
        Losses = losses;
        Accept = accept;
        Decline = decline;
    }
}

[Serializable, NetSerializable]
public enum TraitorUltraRecruitButton : byte
{
    Decline,
    Accept,
}

[Serializable, NetSerializable]
public sealed class TraitorUltraRecruitChoiceMessage : EuiMessageBase
{
    public readonly TraitorUltraRecruitButton Button;

    public TraitorUltraRecruitChoiceMessage(TraitorUltraRecruitButton button)
    {
        Button = button;
    }
}

[Serializable, NetSerializable]
public sealed class TraitorUltraRecruitEuiState : EuiStateBase
{
    public string Title = string.Empty;
    public string Body = string.Empty;
    public string Accept = string.Empty;
    public string Decline = string.Empty;

    public TraitorUltraRecruitEuiState(string title, string body, string accept, string decline)
    {
        Title = title;
        Body = body;
        Accept = accept;
        Decline = decline;
    }
}
