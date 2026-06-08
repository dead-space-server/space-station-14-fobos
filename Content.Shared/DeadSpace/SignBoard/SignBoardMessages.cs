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
