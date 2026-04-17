// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Telephone;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.RedPhone;

[RegisterComponent]
public sealed partial class RedPhoneComponent : Component
{
    [DataField(required: true)]
    public RedPhoneKind Kind = RedPhoneKind.Ordinary;

    [DataField]
    public TimeSpan ReportCooldown = TimeSpan.FromSeconds(300);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextReportTime;

    [ViewVariables(VVAccess.ReadOnly)]
    public string? LastReportedCallerName;

    [ViewVariables(VVAccess.ReadOnly)]
    public string? LastReportedCallerJob;

    [ViewVariables(VVAccess.ReadOnly)]
    public string? LastReportedDeviceName;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan LastReportTime;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool HasReportedThisRound;
}

[Serializable, NetSerializable]
public enum RedPhoneKind : byte
{
    Ordinary,
    CentComm
}

[Serializable, NetSerializable]
public enum RedPhoneUiKey : byte
{
    Key,
    Report
}

[Serializable, NetSerializable]
public sealed class RedPhoneContactEntry
{
    public readonly NetEntity Phone;
    public readonly string Label;
    public readonly bool Available;

    public RedPhoneContactEntry(NetEntity phone, string label, bool available)
    {
        Phone = phone;
        Label = label;
        Available = available;
    }
}

[Serializable, NetSerializable]
public sealed class RedPhoneBoundUiState : BoundUserInterfaceState
{
    public readonly List<RedPhoneContactEntry> Contacts;
    public readonly TelephoneState State;
    public readonly NetEntity? ActivePhone;

    public RedPhoneBoundUiState(List<RedPhoneContactEntry> contacts, TelephoneState state, NetEntity? activePhone)
    {
        Contacts = contacts;
        State = state;
        ActivePhone = activePhone;
    }
}

[Serializable, NetSerializable]
public sealed class RedPhoneStartCallMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Receiver;

    public RedPhoneStartCallMessage(NetEntity receiver)
    {
        Receiver = receiver;
    }
}

[Serializable, NetSerializable]
public sealed class RedPhoneEndCallMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class RedPhoneAnswerCallMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class RedPhoneSubmitReportMessage : BoundUserInterfaceMessage
{
    public readonly string Message;

    public RedPhoneSubmitReportMessage(string message)
    {
        Message = message;
    }
}
