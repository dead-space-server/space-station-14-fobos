using Robust.Shared.GameStates;

namespace Content.Shared.PipeShuttle.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PipeShuttleCallComponent : Component
{
    [DataField("targetStopId")]
    public string TargetStopId = string.Empty;
}
