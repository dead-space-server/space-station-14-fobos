// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Shuttles.Events;

[Serializable, NetSerializable]
public sealed class EmergencyShuttleHijackStartMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class EmergencyShuttleHijackCancelMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class EmergencyShuttleHijackAvailabilityMessage : BoundUserInterfaceMessage
{
    public readonly bool CanStart;
    public readonly bool CanCancel;

    public EmergencyShuttleHijackAvailabilityMessage(bool canStart, bool canCancel)
    {
        CanStart = canStart;
        CanCancel = canCancel;
    }
}
