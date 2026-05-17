using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.GameObjects; //DS14

namespace Content.Shared.Strip.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class StrippableComponent : Component
    {
        /// <summary>
        ///     The strip delay for hands.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField("handDelay")]
        public TimeSpan HandStripDelay = TimeSpan.FromSeconds(4f);
    }

    [NetSerializable, Serializable]
    public enum StrippingUiKey : byte
    {
        Key,
    }

    [NetSerializable, Serializable]
    public sealed class StrippingSlotButtonPressed(string slot, bool isHand) : BoundUserInterfaceMessage
    {
        public readonly string Slot = slot;
        public readonly bool IsHand = isHand;
    }

    [NetSerializable, Serializable]
    public sealed class StrippingEnsnareButtonPressed : BoundUserInterfaceMessage;

    [ByRefEvent]
    public abstract class BaseBeforeStripEvent(TimeSpan initialTime, bool stealth = false) : EntityEventArgs, IInventoryRelayEvent
    {
        public readonly TimeSpan InitialTime = initialTime;
        public float Multiplier = 1f;
        public TimeSpan Additive = TimeSpan.Zero;
        public bool Stealth = stealth;

        public TimeSpan Time => TimeSpan.FromTicks(Math.Max((InitialTime * Multiplier + Additive).Ticks, 0));

        public SlotFlags TargetSlots { get; } = SlotFlags.GLOVES;
    }

    /// <summary>
    ///     Used to modify strip times. Raised directed at the item being stripped.
    /// </summary>
    /// <remarks>
    ///     This is also used by some stripping related interactions, i.e., interactions with items that are currently equipped by another player.
    /// </remarks>
    [ByRefEvent]
    public sealed class BeforeItemStrippedEvent(TimeSpan initialTime, bool stealth = false) : BaseBeforeStripEvent(initialTime, stealth);

    /// <summary>
    ///     Used to modify strip times. Raised directed at the user.
    /// </summary>
    /// <remarks>
    ///     This is also used by some stripping related interactions, i.e., interactions with items that are currently equipped by another player.
    /// </remarks>
    [ByRefEvent]
    public sealed class BeforeStripEvent(TimeSpan initialTime, bool stealth = false) : BaseBeforeStripEvent(initialTime, stealth);

    /// <summary>
    ///     Used to modify strip times. Raised directed at the target.
    /// </summary>
    /// <remarks>
    ///     This is also used by some stripping related interactions, i.e., interactions with items that are currently equipped by another player.
    /// </remarks>
    [ByRefEvent]
    public sealed class BeforeGettingStrippedEvent(TimeSpan initialTime, bool stealth = false) : BaseBeforeStripEvent(initialTime, stealth);

    /// <summary>
    ///     Organizes the behavior of DoAfters for <see cref="StrippableSystem">.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed partial class StrippableDoAfterEvent : DoAfterEvent
    {
        public readonly bool InsertOrRemove;
        public readonly bool InventoryOrHand;
        public readonly string SlotOrHandName;

        public StrippableDoAfterEvent(bool insertOrRemove, bool inventoryOrHand, string slotOrHandName)
        {
            InsertOrRemove = insertOrRemove;
            InventoryOrHand = inventoryOrHand;
            SlotOrHandName = slotOrHandName;
        }

        public override DoAfterEvent Clone() => this;
    }
    [Serializable, NetSerializable] //DS14-start
    public sealed partial class StartStripInsertInventoryMessage : EntityEventArgs
    {
        public string Held { get; set; }
        public string Name { get; set; }
        public int WhoAnswer { get; set; }
        public StartStripInsertInventoryMessage(string held, string name, EntityUid whoAnswer)
        {
            Held = held;
            Name = name;
            WhoAnswer = whoAnswer.Id;
        }
    }
    [Serializable, NetSerializable]
    public sealed partial class AnswerStripInsertInventoryMessage : EntityEventArgs
    {
        public int EUID { get; set; }
        public bool Answer { get; set; }
        public AnswerStripInsertInventoryMessage(int eUID, bool answer)
        {
            EUID = eUID;
            Answer = answer;
        }
    }
    [Serializable, NetSerializable]
    public sealed partial class EndStripInsertInventoryMessage : EntityEventArgs
    {
    }
    //DS14-end
}
