// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Inventory;

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