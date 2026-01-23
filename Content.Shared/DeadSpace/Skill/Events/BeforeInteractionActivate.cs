using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.Skills.Events;

[PublicAPI]
public sealed class BeforeInteractionActivate : HandledEntityEventArgs
{
    /// <summary>
    ///     Entity that triggered the interaction.
    /// </summary>
    public EntityUid User { get; }

    /// <summary>
    ///     Entity that was interacted on.
    /// </summary>
    public EntityUid Used { get; }

    public BeforeInteractionActivate(EntityUid user, EntityUid used)
    {
        User = user;
        Used = used;
    }
}

