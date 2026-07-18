using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.PatrolTablet;

[Serializable, NetSerializable]
public enum PatrolTabletUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PatrolTabletUpdateState(
    List<PatrolOfficerInfo> officers,
    List<PatrolSquadDef> squads)
    : BoundUserInterfaceState
{
    public List<PatrolOfficerInfo> Officers { get; } = officers;
    public List<PatrolSquadDef> Squads { get; } = squads;
}

[Serializable, NetSerializable]
public sealed class PatrolTabletRenameSquadMessage(string squadId, string newName)
    : BoundUserInterfaceMessage
{
    public string SquadId { get; } = squadId;
    public string NewName { get; } = newName;
}

[Serializable, NetSerializable]
public sealed class PatrolTabletBulkAssignSquadMessage(string squadId)
    : BoundUserInterfaceMessage
{
    public string SquadId { get; } = squadId;
}

[Serializable, NetSerializable]
public sealed class PatrolTabletClearAllMessage()
    : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class PatrolTabletClearSquadMessage(string squadId)
    : BoundUserInterfaceMessage
{
    public string SquadId { get; } = squadId;
}

[Serializable, NetSerializable]
public sealed class PatrolTabletCreateSquadMessage(string name, string iconId)
    : BoundUserInterfaceMessage
{
    public string Name { get; } = name;
    public string IconId { get; } = iconId;
}

[Serializable, NetSerializable]
public sealed class PatrolTabletDeleteSquadMessage(string squadId)
    : BoundUserInterfaceMessage
{
    public string SquadId { get; } = squadId;
}
