using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Misc
{
    [RegisterComponent]
    [NetworkedComponent]
    [AutoGenerateComponentState] // ← важно для AutoNetworkedField
    public sealed partial class HarpoonProjectileComponent : Component
    {
        [DataField, AutoNetworkedField]
        public EntityUid? HitTarget; // цель, в которую попал harpoon
    }
}
