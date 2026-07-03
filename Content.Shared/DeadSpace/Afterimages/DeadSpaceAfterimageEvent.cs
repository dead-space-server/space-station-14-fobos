// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Map;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Afterimages;

[Serializable, NetSerializable]
public sealed class DeadSpaceAfterimageEvent : EntityEventArgs
{
    public NetEntity Source;
    public List<NetCoordinates> Coordinates;
    public List<Angle> Rotations;
    public Color Color;
    public float Lifetime;
    public string FallbackEffect;

    public DeadSpaceAfterimageEvent(NetEntity source, List<NetCoordinates> coordinates, List<Angle> rotations, Color color, float lifetime, string fallbackEffect)
    {
        Source = source;
        Coordinates = coordinates;
        Rotations = rotations;
        Color = color;
        Lifetime = lifetime;
        FallbackEffect = fallbackEffect;
    }
}
