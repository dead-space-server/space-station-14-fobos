using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.CodeLock
{
    [Serializable, NetSerializable]
    public enum CodeLockUiKey : byte
    {
        Key
    }

    public enum CodeLockStatus : byte
    {
        AWAIT_CODE,
        UNLOCKED,
        CHANGE,
        COOLDOWN
    }

    [Serializable, NetSerializable]
    public sealed class CodeLockUiState : BoundUserInterfaceState
    {
        public CodeLockStatus Status;
        public string? EnteredCode;
        public int CooldownTime;
        public int EnteredCodeLength;
        public int CodeLength;
        public int MaxCodeLength;
    }
}
