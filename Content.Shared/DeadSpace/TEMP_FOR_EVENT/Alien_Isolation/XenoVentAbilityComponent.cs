using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Shared.DeadSpace.TEMP_FOR_EVENT.Alien_Isolation;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class XenoVentAbilityComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsActive;

    [DataField, AutoNetworkedField]
    public BodyType? OriginalBodyType;

    [DataField("lastValidGridCoords")]
    public EntityCoordinates? LastValidGridCoords;

    [DataField, AutoNetworkedField]
    public BodyStatus? OriginalBodyStatus;

    [DataField, AutoNetworkedField]
    public int? OriginalCollisionLayer;

    [DataField, AutoNetworkedField]
    public int? OriginalCollisionMask;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? OriginalFootstepSound;

    public bool AddedCanMoveInAir;
}
