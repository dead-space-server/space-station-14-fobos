using System.Numerics;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Abilities;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class ActiveRollingStoneComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;

    [DataField, AutoNetworkedField]
    public Vector2 Direction;

    [DataField, AutoNetworkedField]
    public float Speed;

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new();

    [DataField, AutoNetworkedField]
    public bool OldCanMove = true;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? HitSound;

    [DataField, AutoNetworkedField]
    public string? SpritePath;

    [DataField, AutoNetworkedField]
    public string? SpriteState;

    [DataField, AutoNetworkedField]
    public string? OldSpritePath;

    [DataField, AutoNetworkedField]
    public string? OldSpriteState;

    [DataField, AutoNetworkedField]
    public bool SpriteApplied;
}