// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Inventory;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.Virus;

/// <summary>
///     Логика резистов зомби инфекции.
/// </summary>
public sealed class VirusResistanceQueryEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; }
    public float TotalCoefficient = 1.0f;

    public VirusResistanceQueryEvent(SlotFlags slots)
    {
        TargetSlots = slots;
    }
}

[Serializable, NetSerializable]
public sealed partial class CollectVirusDataDoAfterEvent : SimpleDoAfterEvent
{ }
