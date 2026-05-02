using Content.Shared.Actions;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.DeadSpace.Abilities;

public sealed partial class RollingStoneActionEvent : InstantActionEvent
{
    [DataField]
    public float Duration = 8f;

    [DataField]
    public string SpritePath = string.Empty;

    [DataField]
    public string SpriteState = string.Empty;

    [DataField]
    public float Speed = 8f;

    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public SoundSpecifier? HitSound;
}