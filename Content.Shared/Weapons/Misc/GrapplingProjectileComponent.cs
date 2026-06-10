using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Misc;

[RegisterComponent, NetworkedComponent]
public sealed partial class GrapplingProjectileComponent : Component
{
//DS14-start
    [DataField]
    public EntityUid? HitTarget;
//DS14-end
}
