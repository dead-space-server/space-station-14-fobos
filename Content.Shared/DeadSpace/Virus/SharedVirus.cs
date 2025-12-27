// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Inventory;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;
using Content.Shared.Actions;

namespace Content.Shared.Virus;

/// <summary>
///     Логика резистов зомби инфекции.
/// </summary>
public sealed class VirusResistanceQueryEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; }
    public float TotalCoefficient = 1f;

    public VirusResistanceQueryEvent(SlotFlags slots)
    {
        TargetSlots = slots;
    }
}

public sealed class CureVirusEvent : EntityEventArgs
{
    public EntityUid Target { get; }

    public CureVirusEvent(EntityUid target)
    {
        Target = target;
    }
}

public sealed class ProbInfectAttemptEvent : EntityEventArgs
{
    public EntityUid Target { get; }
    public EntityUid? Host { get; }
    public bool Cancel { get; set; }

    public ProbInfectAttemptEvent(EntityUid target, bool cancel = false, EntityUid? host = null)
    {
        Target = target;
        Host = host;
        Cancel = cancel;
    }
}

public sealed class CauseVirusEvent : EntityEventArgs
{
    public EntityUid Target { get; }

    public CauseVirusEvent(EntityUid target)
    {
        Target = target;
    }
}

[NetSerializable, Serializable]
public enum VirusMutationVisuals : byte
{
    state,
    infected
}


[Serializable, NetSerializable]
public sealed partial class CollectVirusDataDoAfterEvent : SimpleDoAfterEvent
{ }


public sealed partial class ShopMutationActionEvent : InstantActionEvent
{

}

public sealed partial class TeleportToPrimaryPatientEvent : InstantActionEvent
{

}
public sealed partial class SelectPrimaryPatientEvent : EntityTargetActionEvent
{

}
