using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class LavalandResonanceFieldComponent : Component
{
    [ViewVariables]
    public EntityUid? Resonator;

    [ViewVariables]
    public EntityUid? Creator;

    [AutoPausedField]
    public TimeSpan BurstAt;

    [ViewVariables]
    public DamageSpecifier Damage = new();

    [ViewVariables]
    public float LavalandDamageMultiplier = 3f;

    [ViewVariables]
    public float DamageRadius = 1.15f;

    [ViewVariables]
    public bool IgnoreCreatorDamage = true;

    [ViewVariables]
    public string? BurstProjectilePrototype;

    [ViewVariables]
    public int BurstProjectileCount;

    [ViewVariables]
    public float BurstProjectileSpeed = 9f;

    [ViewVariables]
    public float BonusYieldChance;

    [ViewVariables]
    public int BonusYieldMultiplier = 1;

    [ViewVariables]
    public SoundSpecifier BurstSound = new SoundPathSpecifier("/Audio/Weapons/plasma_cutter.ogg");

    [ViewVariables]
    public bool Bursting;
}
