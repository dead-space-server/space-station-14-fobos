using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.PatrolTablet;

[RegisterComponent, NetworkedComponent]
public sealed partial class PatrolTabletComponent : Component
{
    [DataField]
    public List<NetEntity> TrackedPersonnel = new();

    [DataField]
    public List<SquadData> Squads = new();
}

[Serializable, NetSerializable]
public sealed class SquadData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IconId { get; set; } = string.Empty;

    public SquadData(string id, string name, string iconId)
    {
        Id = id;
        Name = name;
        IconId = iconId;
    }
}

[Serializable, NetSerializable]
public sealed class PatrolOfficerInfo
{
    public string OfficerId { get; set; }
    public string Name { get; set; }
    public string JobTitle { get; set; }
    public string SquadId { get; set; } = string.Empty;
    public string SquadIcon { get; set; } = "DeadSpaceSquadIconAlpha";

    public PatrolOfficerInfo(
        string officerId,
        string name,
        string jobTitle)
    {
        OfficerId = officerId;
        Name = name;
        JobTitle = jobTitle;
    }
}

[Serializable, NetSerializable]
public sealed class PatrolSquadDef
{
    public string SquadId { get; set; }
    public string Name { get; set; }
    public int AssignedCount { get; set; }
    public string IconId { get; set; }
    public List<string> Members { get; set; } = new();

    public PatrolSquadDef(string squadId, string name, string iconId)
    {
        SquadId = squadId;
        Name = name;
        IconId = iconId;
    }
}
