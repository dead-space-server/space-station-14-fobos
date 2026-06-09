using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandShelterCapsuleComponent : Component
{
    [DataField]
    public ResPath ShelterPath = new("/Maps/_DeadSpace/Lavaland/save_capsule.yml");

    [DataField]
    public int ClearanceRadius = 2;

    [DataField]
    public Vector2 ShelterSpawnOffset = new(1.5f, 1.5f);

    [DataField]
    public float OutpostReservationPadding = 2f;

    [DataField]
    public SoundSpecifier DeploySound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    [DataField]
    public string DeployEffect = "PuddleSparkle";
}
