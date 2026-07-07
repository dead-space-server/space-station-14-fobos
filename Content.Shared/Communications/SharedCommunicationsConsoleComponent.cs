using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.Languages.Prototypes;

namespace Content.Shared.Communications
{
    [Virtual]
    public partial class SharedCommunicationsConsoleComponent : Component;

    [Serializable, NetSerializable]
    public sealed class CommunicationsConsoleInterfaceState : BoundUserInterfaceState
    {
        public readonly bool CanAnnounce;
        public readonly bool CanBroadcast = true;
        public readonly bool CanCall;
        public readonly bool RigtAnswer; //DS14
        public readonly bool PassWordIsNull;
        public string? Password;
        public readonly TimeSpan? ExpectedCountdownEnd;
        public readonly bool CountdownStarted;
        public List<string>? AlertLevels;
        public string CurrentAlert;
        public float CurrentAlertDelay;

        public CommunicationsConsoleInterfaceState(bool canAnnounce, bool canCall, List<string>? alertLevels, string currentAlert, float currentAlertDelay, TimeSpan? expectedCountdownEnd = null, bool rigtAnswer = false, bool passWordIsNull = false, string? password = null)
        {
            CanAnnounce = canAnnounce;
            CanCall = canCall;
            ExpectedCountdownEnd = expectedCountdownEnd;
            CountdownStarted = expectedCountdownEnd != null;
            AlertLevels = alertLevels;
            CurrentAlert = currentAlert;
            CurrentAlertDelay = currentAlertDelay;
            RigtAnswer = rigtAnswer;
            PassWordIsNull = passWordIsNull;
            Password = password;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CommunicationsConsoleSelectAlertLevelMessage : BoundUserInterfaceMessage
    {
        public readonly string Level;

        public CommunicationsConsoleSelectAlertLevelMessage(string level)
        {
            Level = level;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CommunicationsConsoleAnnounceMessage : BoundUserInterfaceMessage
    {
        public readonly string Message;

        public CommunicationsConsoleAnnounceMessage(string message)
        {
            Message = message;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CommunicationsConsoleBroadcastMessage : BoundUserInterfaceMessage
    {
        public readonly string Message;
        public CommunicationsConsoleBroadcastMessage(string message)
        {
            Message = message;
        }
    }

    [Serializable, NetSerializable]
    public sealed class CommunicationsConsoleCallEmergencyShuttleMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class CommunicationsConsoleRecallEmergencyShuttleMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public enum CommunicationsConsoleUiKey
    {
        Key
    }
    [Serializable, NetSerializable]
    public sealed class EmagedAnonce : BoundUserInterfaceMessage
    {
        public bool UseMyTTS = true;
        public string Announcer = default!;
        public string Announcement = default!;
        public ProtoId<LanguagePrototype> LanguageId = default!; // DS14-Languages
        public string Voice = default!; // Corvax-TTS
        public string ColorHex = "1d8bad";
        public string SoundPath = "/Audio/_DeadSpace/Announcements/centcomm.ogg";
        public float SoundVolume = 5f;
        public string Sender = "";
        public string? Password;
        public EmagedAnonce(
        string sender,
        string announcement,
        ProtoId<LanguagePrototype> languageId,
        bool useMyTTS,
        string voice,
        string announcerName,
        string colorHex = "1d8bad",
        string soundPath = "/Audio/_DeadSpace/Announcements/centcomm.ogg",
        float soundVolume = 5f,
        string? password = null)
        {
            Sender = sender;
            Announcement = announcement;
            LanguageId = languageId;
            UseMyTTS = useMyTTS;
            Announcer = announcerName;
            Voice = voice;
            ColorHex = colorHex;
            SoundPath = soundPath;
            SoundVolume = soundVolume;
            Password = password;
        }
        // DS14-announce-end
    }
    [Serializable, NetSerializable]
    public sealed class PasswordSet : BoundUserInterfaceMessage
    {
        public string Password;
        public PasswordSet(string message)
        {
            Password = message;
        }
        // DS14-announce-end
    }
}
