using Robust.Shared.Serialization;

namespace Content.Shared._Frostheim.Shuttle;

[Serializable, NetSerializable]
public enum FrostheimNavConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum NavGamePhase : byte
{
    Pathfinding,
    Calibration,
    Complete
}

[Serializable, NetSerializable]
public enum NavGridCell : byte
{
    Empty,
    Asteroid
}

[Serializable, NetSerializable]
public sealed class FrostheimNavConsoleState : BoundUserInterfaceState
{
    public NavGamePhase Phase;
    public NavGridCell[]? Grid;
    public int GridWidth;
    public int GridHeight;
    public int StartX;
    public int StartY;
    public int EndX;
    public int EndY;
    public List<(int X, int Y)> CurrentPath = new();
    public bool PathComplete;
    public float[] TargetValues = Array.Empty<float>();
    public float[] CurrentValues = Array.Empty<float>();
    public float Tolerance;
}

[Serializable, NetSerializable]
public sealed class NavConsolePathClickMessage : BoundUserInterfaceMessage
{
    public int X;
    public int Y;

    public NavConsolePathClickMessage(int x, int y)
    {
        X = x;
        Y = y;
    }
}

[Serializable, NetSerializable]
public sealed class NavConsoleCalibrateMessage : BoundUserInterfaceMessage
{
    public int SliderIndex;
    public float Value;

    public NavConsoleCalibrateMessage(int sliderIndex, float value)
    {
        SliderIndex = sliderIndex;
        Value = value;
    }
}

[Serializable, NetSerializable]
public sealed class NavConsoleConfirmMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class NavConsoleResetPathMessage : BoundUserInterfaceMessage;
