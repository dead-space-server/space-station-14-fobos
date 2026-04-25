using System.Numerics; //DS14
using Content.Shared.Shuttles.BUIStates; //DS14
using Content.Shared.DeadSpace.Shuttles.Components; //DS14
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class NavInterfaceState
{
    public float MaxRange;

    /// <summary>
    /// The relevant coordinates to base the radar around.
    /// </summary>
    public NetCoordinates? Coordinates;

    /// <summary>
    /// The relevant rotation to rotate the angle around.
    /// </summary>
    public Angle? Angle;

    public Dictionary<NetEntity, List<DockingPortState>> Docks;

    public bool RotateWithEntity = true;

    //DS14-start
    public List<BlipState> Blips;
    //DS14-end

    public NavInterfaceState(
        float maxRange,
        NetCoordinates? coordinates,
        Angle? angle,
        Dictionary<NetEntity, List<DockingPortState>> docks,
        List<BlipState>? blips = null) //DS14
    {
        MaxRange = maxRange;
        Coordinates = coordinates;
        Angle = angle;
        Docks = docks;
        Blips = blips ?? new List<BlipState>(); //DS14
    }
}

[Serializable, NetSerializable]
public enum RadarConsoleUiKey : byte
{
    Key
}

//DS14-start
[Serializable, NetSerializable]
public sealed class BlipState
{
    public Vector2 WorldPosition;
    public Color Color;
    public float Radius;

    public BlipState(Vector2 worldPosition, Color color, float radius = 0.5f)
    {
        WorldPosition = worldPosition;
        Color = color;
        Radius = radius;
    }
}
//DS14-end