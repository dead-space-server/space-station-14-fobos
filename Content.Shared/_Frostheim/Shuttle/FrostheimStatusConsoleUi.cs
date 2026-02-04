using Robust.Shared.Serialization;

namespace Content.Shared._Frostheim.Shuttle;

[Serializable, NetSerializable]
public enum FrostheimStatusConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class FrostheimStatusConsoleState : BoundUserInterfaceState
{
    public int ThrustersNeeded;
    public int ThrustersCurrent;
    public bool GyroscopeRequired;
    public bool GyroscopeInstalled;
    public bool NavigationComplete;
    public bool Ready;
}
