using Content.Shared._Frostheim.Shuttle;
using Robust.Shared.Serialization;

namespace Content.Shared._Frostheim.Radar;

[Serializable, NetSerializable]
public enum FrostheimRadarUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class FrostheimRadarUpdateMessage : BoundUserInterfaceMessage
{
    public List<RadarBlip> Blips;
    public int TotalThrusters;
    public int TotalGyroscopes;
    public float MaxRange;

    public FrostheimRadarUpdateMessage(List<RadarBlip> blips, int totalThrusters, int totalGyroscopes, float maxRange)
    {
        Blips = blips;
        TotalThrusters = totalThrusters;
        TotalGyroscopes = totalGyroscopes;
        MaxRange = maxRange;
    }
}

[Serializable, NetSerializable]
public sealed class RadarBlip
{
    public FrostheimPartType Type;
    public float Angle;
    public float Distance;

    public RadarBlip(FrostheimPartType type, float angle, float distance)
    {
        Type = type;
        Angle = angle;
        Distance = distance;
    }
}
