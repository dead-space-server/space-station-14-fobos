// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.ERT
{
    [Serializable, NetSerializable]
    public sealed class RequestErtAdminStateMessage : EntityEventArgs
    {
    }

    [Serializable, NetSerializable]
    public sealed class ErtAdminEntry
    {
        public string ProtoId { get; }
        public string Name { get; }
        public int SecondsRemaining { get; }
        public int Price { get; }
        public string? CallReason { get; }

        public ErtAdminEntry(string protoId, string name, int secondsRemaining, int price, string? callReason = null)
        {
            ProtoId = protoId;
            Name = name;
            SecondsRemaining = secondsRemaining;
            Price = price;
            CallReason = callReason;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ErtAdminStateResponse : EntityEventArgs
    {
        public ErtAdminEntry[] Entries { get; }
        public int Points { get; }
        public int CooldownSeconds { get; }

        public ErtAdminStateResponse(ErtAdminEntry[] entries, int points, int cooldownSeconds)
        {
            Entries = entries;
            Points = points;
            CooldownSeconds = cooldownSeconds;
        }
    }

    [Serializable, NetSerializable]
    public sealed class AdminModifyErtEntryMessage : EntityEventArgs
    {
        public string ProtoId { get; }
        public int Seconds { get; }

        public AdminModifyErtEntryMessage(string protoId, int seconds)
        {
            ProtoId = protoId;
            Seconds = seconds;
        }
    }

    [Serializable, NetSerializable]
    public sealed class AdminDeleteErtMessage : EntityEventArgs
    {
        public string ProtoId { get; }

        public AdminDeleteErtMessage(string protoId)
        {
            ProtoId = protoId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class AdminSetPointsMessage : EntityEventArgs
    {
        public int Points { get; }

        public AdminSetPointsMessage(int points)
        {
            Points = points;
        }
    }

    [Serializable, NetSerializable]
    public sealed class AdminSetCooldownMessage : EntityEventArgs
    {
        public int Seconds { get; }

        public AdminSetCooldownMessage(int seconds)
        {
            Seconds = seconds;
        }
    }

    [Serializable, NetSerializable]
    public sealed class AdminSetErtReasonMessage : EntityEventArgs
    {
        public string ProtoId { get; }
        public string Reason { get; }

        public AdminSetErtReasonMessage(string protoId, string reason)
        {
            ProtoId = protoId;
            Reason = reason;
        }
    }

    [Serializable, NetSerializable]
    public sealed class AdminCallErtMessage : EntityEventArgs
    {
        public string ProtoId { get; }
        public string Reason { get; }

        public AdminCallErtMessage(string protoId, string reason)
        {
            ProtoId = protoId;
            Reason = reason;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ErtAdminActionResult : EntityEventArgs
    {
        public bool Success { get; }
        public string Message { get; }

        public ErtAdminActionResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
