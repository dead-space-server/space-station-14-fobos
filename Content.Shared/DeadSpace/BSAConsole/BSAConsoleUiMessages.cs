using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.BSAConsole;

[Serializable, NetSerializable]
public enum BSAConsoleUiKey : byte { Key }

[Serializable, NetSerializable]
public sealed class BSAConsoleFireMessage : BoundUserInterfaceMessage
{
    public float X { get; }
    public float Y { get; }
    public int MapId { get; }

    public BSAConsoleFireMessage(float x, float y, int mapId)
    {
        X = x;
        Y = y;
        MapId = mapId;
    }
}

[Serializable, NetSerializable]
public sealed class BSAConsoleSwitchViewMessage : BoundUserInterfaceMessage
{
    public string ViewMode { get; }

    public BSAConsoleSwitchViewMessage(string viewMode)
    {
        ViewMode = viewMode;
    }
}

[Serializable, NetSerializable]
public sealed class BSAConsoleSelectGridMessage : BoundUserInterfaceMessage
{
    public string GridName { get; }

    public BSAConsoleSelectGridMessage(string gridName)
    {
        GridName = gridName;
    }
}

[Serializable, NetSerializable]
public sealed class BSAConsoleEjectDiskMessage : BoundUserInterfaceMessage { }
