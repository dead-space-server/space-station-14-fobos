using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class ScannerProgramUiMessageEvent : CartridgeMessageEvent
{
    public readonly ScannerProgramUiAction Action;
    public readonly int DocumentIndex;
    public readonly string? NewName;

    public ScannerProgramUiMessageEvent(ScannerProgramUiAction action, int documentIndex = -1, string? newName = null)
    {
        Action = action;
        DocumentIndex = documentIndex;
        NewName = newName;
    }
}

[Serializable, NetSerializable]
public enum ScannerProgramUiAction
{
    Select,
    Delete,
    Clear,
    Rename
}
