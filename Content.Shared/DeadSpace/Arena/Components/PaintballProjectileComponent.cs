using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.Arena;

/// <summary>
/// Маркер пули пеинтболла: при попадании hitscan-выстрела сервер ставит на пол
/// шлейф краски вдоль траектории полёта.
/// </summary>
[RegisterComponent]
public sealed partial class PaintballProjectileComponent : Component
{
    /// <summary>Ширина шлейфа краски (радиус вокруг линии полёта), в тайлах.</summary>
    [DataField]
    public float TrailWidth = 0.5f;

    /// <summary>Радиус пятна краски в точке попадания, в тайлах (0 — без пятна).</summary>
    [DataField]
    public float ImpactRadius = 0.5f;
}