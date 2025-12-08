using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.IPC;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOnEMPComponent : Component
{
    [DataField]
    public float Damage = 10f;

    [DataField]
    public ProtoId<DamageTypePrototype> DamageType = "Shock";
}
