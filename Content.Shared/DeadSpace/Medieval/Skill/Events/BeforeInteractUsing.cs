using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.Medieval.Skills.Events;

[PublicAPI]
public sealed class BeforeInteractUsingEvent : HandledEntityEventArgs
{
    /// <summary>
    ///     Entity that triggered the interaction.
    /// </summary>
    public EntityUid User { get; }

    /// <summary>
    ///     Entity that the user used to interact.
    /// </summary>
    public EntityUid Used { get; }

    /// <summary>
    ///     Entity that was interacted on.
    /// </summary>
    public EntityUid Target { get; }

    public BeforeInteractUsingEvent(EntityUid user, EntityUid used, EntityUid target)
    {
        DebugTools.Assert(used != target);

        User = user;
        Used = used;
        Target = target;
    }
}

