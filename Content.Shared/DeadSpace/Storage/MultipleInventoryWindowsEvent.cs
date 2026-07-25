using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Storage;

[Serializable, NetSerializable]
public sealed class MultipleInventoryWindowsEnabledEvent : EntityEventArgs
{
    public bool Enabled { get; }

    public MultipleInventoryWindowsEnabledEvent(bool enabled)
    {
        Enabled = enabled;
    }
}
