using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Shuttles.BUIStates;

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