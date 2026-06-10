// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.SignBoard;

[Serializable, NetSerializable]
public sealed class SignBoardSetTextMessage : BoundUserInterfaceMessage
{
    public string Text { get; }

    public SignBoardSetTextMessage(string text)
    {
        Text = text;
    }
}

[Serializable, NetSerializable]
public sealed class SignBoardBoundUserInterfaceState : BoundUserInterfaceState
{
    public string Text { get; }

    public SignBoardBoundUserInterfaceState(string text)
    {
        Text = text;
    }
}
