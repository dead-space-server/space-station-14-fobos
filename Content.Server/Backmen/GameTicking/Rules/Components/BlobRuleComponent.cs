using Content.Server.Backmen.Blob;
using Content.Shared.Mind;
using Content.Server.DeadSpace.Administration;
using Robust.Shared.Audio;

namespace Content.Server.Backmen.GameTicking.Rules.Components;

// DS14-start
[RegisterComponent, Access(typeof(BlobRuleSystem), typeof(BlobCoreSystem), typeof(BlobObserverSystem), typeof(BlobAntagRollbackSystem))]
// DS14-end
public sealed partial class BlobRuleComponent : Component
{
    public List<(EntityUid mindId, MindComponent mind)> Blobs = new(); //BlobRoleComponent

    public BlobStage Stage = BlobStage.Default;

    [DataField("alertAodio")]
    public SoundSpecifier? AlertAudio = new SoundPathSpecifier("/Audio/_DeadSpace/Announcements/attention.ogg"); // DS14-Announcements

    public float Accumulator = 0f;
}


public enum BlobStage : byte
{
    Default,
    Begin,
    Medium,
    Critical,
    TheEnd
}
