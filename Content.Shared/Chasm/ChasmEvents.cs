namespace Content.Shared.Chasm;

public sealed class ChasmFallingAttemptEvent(EntityUid tripper, EntityUid chasm) : CancellableEntityEventArgs
{
    public EntityUid Tripper { get; } = tripper;

    public EntityUid Chasm { get; } = chasm;
}
