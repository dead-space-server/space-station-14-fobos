using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.ERT;

[Serializable, NetSerializable]
public sealed class AdminErtEuiState : EuiStateBase
{
    // No state for now; window queries system itself on open.
}

public static class AdminErtEuiMsg
{
    [Serializable, NetSerializable]
    public sealed class Close : EuiMessageBase
    {
    }
}
