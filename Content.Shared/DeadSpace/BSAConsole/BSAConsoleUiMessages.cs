using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.BSAConsole;

[Serializable, NetSerializable]
public enum BSAConsoleUiKey : byte { Key }

/// <summary>
/// Fire at absolute map coordinates (X, Y).
/// </summary>
[Serializable, NetSerializable]
public sealed class BSAConsoleFireMessage : BoundUserInterfaceMessage
{
    public float X;
    public float Y;

    public BSAConsoleFireMessage(float x, float y)
    {
        X = x;
        Y = y;
    }
}

[Serializable, NetSerializable]
public sealed class BSAConsoleSwitchViewMessage : BoundUserInterfaceMessage
{
    public string ViewMode;

    public BSAConsoleSwitchViewMessage(string viewMode)
    {
        ViewMode = viewMode;
    }
}

/// <summary>
/// Select a grid by name for map view. Server resolves name to EntityUid.
/// </summary>
[Serializable, NetSerializable]
public sealed class BSAConsoleSelectGridMessage : BoundUserInterfaceMessage
{
    public string GridName;

    public BSAConsoleSelectGridMessage(string gridName)
    {
        GridName = gridName;
    }
}

[Serializable, NetSerializable]
public sealed class BSAConsoleEjectDiskMessage : BoundUserInterfaceMessage { }
