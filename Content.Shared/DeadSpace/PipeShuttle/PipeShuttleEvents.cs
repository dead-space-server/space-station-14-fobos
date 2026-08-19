using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.PipeShuttle;

[Serializable, NetSerializable]
public enum PipeShuttleUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class PipeShuttleCallMessage : BoundUserInterfaceMessage
{
    public string DestId = string.Empty;
}

[Serializable, NetSerializable]
public sealed class PipeShuttleDestInfo
{
    public string Id = string.Empty;
    public string Name = string.Empty;
}

[Serializable, NetSerializable]
public sealed class PipeShuttleUiState : BoundUserInterfaceState
{
    public List<PipeShuttleDestInfo> Destinations = new();
    public string? CurrentDestId;
    public bool Travelling;
    public string? TargetDestId;
}
