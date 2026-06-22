using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class EmergencyConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// null if we're not early launching.
    /// </summary>
    public TimeSpan? EarlyLaunchTime;
    public List<string> Authorizations = new();
    public int AuthorizationsRequired;
    public bool EarlyLaunchAllowed; // DS14

    public TimeSpan? TimeToLaunch;

    // DS14-start
    public TimeSpan? HijackCompletionTime;
    public bool HijackCompleted;
    public string HijackerName = string.Empty;
    // DS14-end
}
