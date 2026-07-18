using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;

namespace Content.Shared.DeadSpace.ShieldDash.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShieldDashComponent : Component
{
    [DataField]
    public int NeedFreeHands = 1;

    [DataField, AutoNetworkedField]
    public EntityUid? User;

    public const string DashFixtureID = "shield-dash";

    [DataField]
    public IPhysShape Shape = new PhysShapeCircle(0.4f);

    [DataField]
    public bool IgnoreResistances = false;

    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier Damage = default!;

    [DataField, AutoNetworkedField]
    public DamageSpecifier DamageToShield = default!;

    [DataField]
    public float DisarmChance = 0.1f;

    [DataField]
    public float StaminaDamage = 0;

    [DataField]
    public float StaminaDamageToUser = 0;

    [DataField]
    public EntProtoId DashAction = "ShieldDashAction";

    [DataField, AutoNetworkedField]
    public EntityUid? DashActionEntity;

    [DataField]
    public float DashLength = 3;

    [DataField]
    public float DashSpeed = 7;

    [DataField]
    public SoundSpecifier? DashSound;

    [DataField]
    public SoundSpecifier? ImpactSound =
        new SoundPathSpecifier("/Audio/Weapons/block_metal1.ogg")
        {
            Params = AudioParams.Default.WithVariation(0.25f)
        };
}

public sealed partial class ShieldDashEvent : InstantActionEvent;