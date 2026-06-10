using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.CodeLock
{
    public abstract partial class SharedCodeLockComponent : Component
    {
        public static readonly TimeSpan EnterCodeCooldown = TimeSpan.FromSeconds(1);
    }

    [Serializable, NetSerializable]
    public sealed class CodeLockKeypadMessage : BoundUserInterfaceMessage
    {
        public int Value;

        public CodeLockKeypadMessage(int value)
        {
            Value = value;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CodeLockKeypadClearMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class CodeLockKeypadEnterMessage : BoundUserInterfaceMessage
    {
    }
}
