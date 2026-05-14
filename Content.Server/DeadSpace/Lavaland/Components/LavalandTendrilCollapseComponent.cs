using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class LavalandTendrilCollapseComponent : Component
{
    [DataField]
    public EntProtoId ChasmPrototype = "FloorChasmEntity";

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan CollapseTime = TimeSpan.Zero;

    [DataField]
    public int Radius = 2;

    [DataField]
    public bool SkipCenter = false;

    [DataField]
    public SoundSpecifier CollapseSound = new SoundPathSpecifier("/Audio/_DeadSpace/Lavaland/tendril_destroyed.ogg");

    public bool TendrilCollapseInitialized;
}
