using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.SleepyCarp;

[RegisterComponent]
public sealed partial class GiveSmokingCarpComponent : Component
{
    [DataField]
    public float InjectTime = 5f;

    [DataField]
    public EntProtoId Animation = "WeaponArcBite";

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public SoundSpecifier HitSound = new SoundPathSpecifier("/Audio/Effects/bite.ogg");
}
