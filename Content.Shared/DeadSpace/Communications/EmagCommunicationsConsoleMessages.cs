// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Languages.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Communications;

public static class EmagCommunicationsConsoleConstants
{
    public const int MaxPasswordLength = 10;
    public const int MaxAttributionLength = 32;
}

[Serializable, NetSerializable]
public enum EmagCommunicationsUiMode : byte
{
    Unavailable,
    PasswordSetup,
    Locked,
    Authorized,
}

[Serializable, NetSerializable]
public enum EmagCommunicationsUiError : byte
{
    None,
    InvalidPasswordFormat,
    IncorrectPassword,
    PasswordAlreadySet,
    Unavailable,
    InvalidRequest,
    Cooldown,
    InvalidAnnouncement,
    InvalidSound,
    InvalidLanguage,
    InvalidVoice,
}

[Serializable, NetSerializable]
public sealed class EmagCommunicationsConsoleRequestAccessStateMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class EmagCommunicationsConsoleSetPasswordMessage(string password) : BoundUserInterfaceMessage
{
    public readonly string Password = password;
}

[Serializable, NetSerializable]
public sealed class EmagCommunicationsConsoleUnlockMessage(string password) : BoundUserInterfaceMessage
{
    public readonly string Password = password;
}

[Serializable, NetSerializable]
public sealed class EmagCommunicationsConsoleAnnounceMessage(
    string announcer,
    string signature,
    string announcement,
    ProtoId<LanguagePrototype> languageId,
    bool enableTts,
    bool useCustomTts,
    string voiceId,
    string colorHex,
    string soundPath) : BoundUserInterfaceMessage
{
    public readonly string Announcer = announcer;
    public readonly string Signature = signature;
    public readonly string Announcement = announcement;
    public readonly ProtoId<LanguagePrototype> LanguageId = languageId;
    public readonly bool EnableTts = enableTts;
    public readonly bool UseCustomTts = useCustomTts;
    public readonly string VoiceId = voiceId;
    public readonly string ColorHex = colorHex;
    public readonly string SoundPath = soundPath;
}

[Serializable, NetSerializable]
public sealed class EmagCommunicationsConsoleAccessStateMessage(
    EmagCommunicationsUiMode mode,
    EmagCommunicationsUiError error,
    bool canAnnounce) : BoundUserInterfaceMessage
{
    public readonly EmagCommunicationsUiMode Mode = mode;
    public readonly EmagCommunicationsUiError Error = error;
    public readonly bool CanAnnounce = canAnnounce;
}
