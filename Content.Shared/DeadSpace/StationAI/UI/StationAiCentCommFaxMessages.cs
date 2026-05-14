// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.StationAI.UI;

public static class StationAiCentCommFaxConstants
{
    public const int MaxContentLength = 2000;
}

[Serializable, NetSerializable]
public sealed class StationAiCentCommFaxUiState : BoundUserInterfaceState
{
    public bool CanSend;
    public int CooldownRemainingSeconds;
    public string Status;
    public bool ClearForm;

    public StationAiCentCommFaxUiState(
        bool canSend,
        int cooldownRemainingSeconds,
        string status,
        bool clearForm = false)
    {
        CanSend = canSend;
        CooldownRemainingSeconds = cooldownRemainingSeconds;
        Status = status;
        ClearForm = clearForm;
    }
}

[Serializable, NetSerializable]
public sealed class StationAiCentCommFaxSendMessage : BoundUserInterfaceMessage
{
    public string Content;

    public StationAiCentCommFaxSendMessage(string content)
    {
        Content = content;
    }
}
