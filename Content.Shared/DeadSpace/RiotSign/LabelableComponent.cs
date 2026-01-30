using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.RiotSign;

[RegisterComponent]
public sealed partial class LabelableComponent : Component
{
    [DataField("originalName")]
    public string? OriginalName;

    [DataField("currentLabel")]
    public string CurrentLabel = string.Empty;
}

[Serializable, NetSerializable]
public enum LabelUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class LabelChangedMessage : BoundUserInterfaceMessage
{
    public string Text { get; }
    public LabelChangedMessage(string text)
    {
        Text = text;
    }
}
