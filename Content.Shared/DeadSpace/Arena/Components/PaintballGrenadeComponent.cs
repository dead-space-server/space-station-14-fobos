using Content.Shared.Damage;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.Arena;

/// <summary>
/// Маркер гранаты пеинтболла: при детонации красит пол в радиусе и наносит урон игрокам.
/// </summary>
[RegisterComponent]
public sealed partial class PaintballGrenadeComponent : Component
{
    /// <summary>Радиус закрашивания пола, в тайлах.</summary>
    [DataField]
    public float PaintRadius = 4f;

    /// <summary>Радиус урона по игрокам, в тайлах.</summary>
    [DataField]
    public float DamageRadius = 3f;

    /// <summary>Урон по игрокам в радиусе.</summary>
    [DataField]
    public DamageSpecifier Damage = new();
}