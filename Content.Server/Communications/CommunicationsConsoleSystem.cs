using Content.Server.Administration.Logs;
using Content.Server.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Screens.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Communications;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Content.Shared.DeadSpace.Languages.Components;
using Content.Server.DeadSpace.Languages;
using Content.Shared.Corvax.TTS;
using Content.Shared.Emag.Systems;
using Content.Shared.Emag.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.ContentPack;

namespace Content.Server.Communications
{
    public sealed class CommunicationsConsoleSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
        [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
        [Dependency] private readonly EmergencyShuttleSystem _emergency = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
        [Dependency] private readonly StationSystem _stationSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly EmagSystem _emag = default!; //DS14
        [Dependency] private readonly IResourceManager _resourceManager = default!; //DS14
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!; //DS14
        private const float UIUpdateInterval = 5.0f;

        public override void Initialize()
        {
            // All events that refresh the BUI
            SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertLevelChanged);
            SubscribeLocalEvent<RoundEndSystemChangedEvent>(_ => OnGenericBroadcastEvent());
            SubscribeLocalEvent<AlertLevelDelayFinishedEvent>(_ => OnGenericBroadcastEvent());

            // Messages from the BUI
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleSelectAlertLevelMessage>(OnSelectAlertLevelMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleAnnounceMessage>(OnAnnounceMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleBroadcastMessage>(OnBroadcastMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleCallEmergencyShuttleMessage>(OnCallShuttleMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleRecallEmergencyShuttleMessage>(OnRecallShuttleMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, EmagedAnonce>(OnEmagAnnounceMessage);//DS14
            SubscribeLocalEvent<CommunicationsConsoleComponent, PasswordSet>(PasswordSet); //DS14

            // On console init, set cooldown
            SubscribeLocalEvent<CommunicationsConsoleComponent, MapInitEvent>(OnCommunicationsConsoleMapInit);

            SubscribeLocalEvent<CommunicationsConsoleComponent, GotEmaggedEvent>(OnEmagged); //DS14
        }

        public override void Update(float frameTime)
        {
            var query = EntityQueryEnumerator<CommunicationsConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                // TODO refresh the UI in a less horrible way
                if (comp.AnnouncementCooldownRemaining >= 0f)
                {
                    comp.AnnouncementCooldownRemaining -= frameTime;
                }

                comp.UIUpdateAccumulator += frameTime;

                if (comp.UIUpdateAccumulator < UIUpdateInterval)
                    continue;

                comp.UIUpdateAccumulator -= UIUpdateInterval;

                if (_uiSystem.IsUiOpen(uid, CommunicationsConsoleUiKey.Key))
                    UpdateCommsConsoleInterface(uid, comp);
            }

            base.Update(frameTime);
        }

        public void OnCommunicationsConsoleMapInit(EntityUid uid, CommunicationsConsoleComponent comp, MapInitEvent args)
        {
            comp.AnnouncementCooldownRemaining = comp.InitialDelay;
            UpdateCommsConsoleInterface(uid, comp);
        }

        /// <summary>
        /// Update the UI of every comms console.
        /// </summary>
        private void OnGenericBroadcastEvent()
        {
            var query = EntityQueryEnumerator<CommunicationsConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                UpdateCommsConsoleInterface(uid, comp);
            }
        }

        /// <summary>
        /// Updates all comms consoles belonging to the station that the alert level was set on
        /// </summary>
        /// <param name="args">Alert level changed event arguments</param>
        private void OnAlertLevelChanged(AlertLevelChangedEvent args)
        {
            var query = EntityQueryEnumerator<CommunicationsConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                var entStation = _stationSystem.GetOwningStation(uid);
                if (args.Station == entStation)
                    UpdateCommsConsoleInterface(uid, comp);
            }
        }

        /// <summary>
        /// Updates the UI for all comms consoles.
        /// </summary>
        public void UpdateCommsConsoleInterface()
        {
            var query = EntityQueryEnumerator<CommunicationsConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                UpdateCommsConsoleInterface(uid, comp);
            }
        }

        /// <summary>
        /// Updates the UI for a particular comms console.
        /// </summary>
        public void UpdateCommsConsoleInterface(EntityUid uid, CommunicationsConsoleComponent comp, bool rightPassword = false)
        {
            var stationUid = _stationSystem.GetOwningStation(uid);
            List<string>? levels = null;
            string currentLevel = default!;
            float currentDelay = 0;

            if (stationUid != null)
            {
                if (TryComp(stationUid.Value, out AlertLevelComponent? alertComp) &&
                    alertComp.AlertLevels != null)
                {
                    if (alertComp.IsSelectable)
                    {
                        levels = new();
                        foreach (var (id, detail) in alertComp.AlertLevels.Levels)
                        {
                            if (detail.Selectable)
                            {
                                levels.Add(id);
                            }
                        }
                    }

                    currentLevel = alertComp.CurrentLevel;
                    currentDelay = _alertLevelSystem.GetAlertLevelDelay(stationUid.Value, alertComp);
                }
            }
            var passWordIsNull = false;
            if (comp.PassWord == null && HasComp<EmaggedComponent>(uid))
            {
                passWordIsNull = true;
            }
            string? _password = null;
            if (rightPassword && HasComp<EmaggedComponent>(uid))
            {
                _password = comp.PassWord;
            }
            _uiSystem.SetUiState(uid, CommunicationsConsoleUiKey.Key, new CommunicationsConsoleInterfaceState(
                CanAnnounce(comp),
                CanCallOrRecall(comp),
                levels,
                currentLevel,
                currentDelay,
                _roundEndSystem.ExpectedCountdownEnd,
                rightPassword,
                passWordIsNull,
                _password
            ));
        }

        private static bool CanAnnounce(CommunicationsConsoleComponent comp)
        {
            return comp.AnnouncementCooldownRemaining <= 0f;
        }

        private bool CanUse(EntityUid user, EntityUid console)
        {
            if (TryComp<AccessReaderComponent>(console, out var accessReaderComponent))
            {
                return _accessReaderSystem.IsAllowed(user, console, accessReaderComponent);
            }
            return true;
        }

        private bool CanCallOrRecall(CommunicationsConsoleComponent comp)
        {
            // Defer to what the round end system thinks we should be able to do.
            if (_emergency.EmergencyShuttleArrived || !_roundEndSystem.CanCallOrRecall())
                return false;

            // Ensure that we can communicate with the shuttle (either call or recall)
            if (!comp.CanShuttle)
                return false;

            // Calling shuttle checks
            if (_roundEndSystem.ExpectedCountdownEnd is null)
                return true;

            // Recalling shuttle checks
            var recallThreshold = _cfg.GetCVar(CCVars.EmergencyRecallTurningPoint);

            // shouldn't really be happening if we got here
            if (_roundEndSystem.ShuttleTimeLeft is not { } left
                || _roundEndSystem.ExpectedShuttleLength is not { } expected)
                return false;

            return !(left.TotalSeconds / expected.TotalSeconds < recallThreshold);
        }

        private void OnSelectAlertLevelMessage(EntityUid uid, CommunicationsConsoleComponent comp, CommunicationsConsoleSelectAlertLevelMessage message)
        {
            if (message.Actor is not { Valid: true } mob)
                return;

            if (!CanUse(mob, uid))
            {
                _popupSystem.PopupCursor(Loc.GetString("comms-console-permission-denied"), message.Actor, PopupType.Medium);
                return;
            }

            var stationUid = _stationSystem.GetOwningStation(uid);
            if (stationUid != null)
            {
                _alertLevelSystem.SetLevel(stationUid.Value, message.Level, true, true);
            }
        }

        private void OnAnnounceMessage(EntityUid uid, CommunicationsConsoleComponent comp,
            CommunicationsConsoleAnnounceMessage message)
        {
            var maxLength = _cfg.GetCVar(CCVars.ChatMaxAnnouncementLength);
            var msg = SharedChatSystem.SanitizeAnnouncement(message.Message, maxLength);
            string originalMessage = msg; // DS14-TTS
            var author = Loc.GetString("comms-console-announcement-unknown-sender");
            if (message.Actor is { Valid: true } mob)
            {
                if (!CanAnnounce(comp))
                {
                    return;
                }

                if (!CanUse(mob, uid))
                {
                    _popupSystem.PopupEntity(Loc.GetString("comms-console-permission-denied"), uid, message.Actor);
                    return;
                }

                var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(uid, mob);
                RaiseLocalEvent(tryGetIdentityShortInfoEvent);
                author = tryGetIdentityShortInfoEvent.Title;
            }

            comp.AnnouncementCooldownRemaining = comp.Delay;
            UpdateCommsConsoleInterface(uid, comp);

            var ev = new CommunicationConsoleAnnouncementEvent(uid, comp, msg, message.Actor);
            RaiseLocalEvent(ref ev);

            // allow admemes with vv
            Loc.TryGetString(comp.Title, out var title);
            title ??= comp.Title;

            // DS14-Languages-start
            var languageId = LanguageSystem.DefaultLanguageId;
            var voice = string.Empty;

            if (TryComp<LanguageComponent>(message.Actor, out var languageComponent))
                languageId = languageComponent.SelectedLanguage;

            if (TryComp<TTSComponent>(message.Actor, out var tts))
                voice = tts.VoicePrototypeId;
            // DS14-Languages-end

            if (comp.AnnounceSentBy)
                msg += "\n" + Loc.GetString("comms-console-announcement-sent-by") + " " + author;

            if (comp.Global)
            {
                _chatSystem.DispatchGlobalAnnouncement(msg, title, announcementSound: comp.Sound, colorOverride: comp.Color, originalMessage: originalMessage, author: message.Actor, languageId: languageId); // DS14-TTS

                _adminLogger.Add(LogType.Chat, LogImpact.Low, $"{ToPrettyString(message.Actor):player} has sent the following global announcement: {msg}");
                return;
            }

            _chatSystem.DispatchStationAnnouncement(uid,
                msg,
                title,
                announcementSound:
                comp.Sound,
                colorOverride:
                comp.Color,
                voice: voice,
                languageId: languageId);

            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"{ToPrettyString(message.Actor):player} has sent the following station announcement: {msg}");

        }

        private void OnBroadcastMessage(EntityUid uid, CommunicationsConsoleComponent component, CommunicationsConsoleBroadcastMessage message)
        {
            var msg = SharedChatSystem.SanitizeAnnouncement(message.Message, _cfg.GetCVar(CCVars.ChatMaxAnnouncementLength)); //DS14=start
            if (HasComp<EmaggedComponent>(uid) && msg == component.PassWord)
            {
                UpdateCommsConsoleInterface(uid, component, true);
                return;
            } //DS14-end
            if (!TryComp<DeviceNetworkComponent>(uid, out var net))
                return;

            var payload = new NetworkPayload
            {
                [ScreenMasks.Text] = msg //DS14
            };

            _deviceNetworkSystem.QueuePacket(uid, null, payload, net.TransmitFrequency);

            _adminLogger.Add(LogType.DeviceNetwork, LogImpact.Low, $"{ToPrettyString(message.Actor):player} has sent the following broadcast: {msg:msg}"); //DS14
        }

        private void OnCallShuttleMessage(EntityUid uid, CommunicationsConsoleComponent comp, CommunicationsConsoleCallEmergencyShuttleMessage message)
        {
            if (!CanCallOrRecall(comp))
                return;

            var mob = message.Actor;

            if (!CanUse(mob, uid))
            {
                _popupSystem.PopupEntity(Loc.GetString("comms-console-permission-denied"), uid, message.Actor);
                return;
            }

            var ev = new CommunicationConsoleCallShuttleAttemptEvent(uid, comp, mob);
            RaiseLocalEvent(ref ev);
            if (ev.Cancelled)
            {
                _popupSystem.PopupEntity(ev.Reason ?? Loc.GetString("comms-console-shuttle-unavailable"), uid, message.Actor);
                return;
            }

            _roundEndSystem.RequestRoundEnd(mob, uid);
            _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(mob):player} has called the shuttle.");
        }

        private void OnRecallShuttleMessage(EntityUid uid, CommunicationsConsoleComponent comp, CommunicationsConsoleRecallEmergencyShuttleMessage message)
        {
            if (!CanCallOrRecall(comp))
                return;

            var mob = message.Actor;

            if (!CanUse(mob, uid))
            {
                _popupSystem.PopupEntity(Loc.GetString("comms-console-permission-denied"), uid, message.Actor);
                return;
            }

            _roundEndSystem.CancelRoundEndCountdown(mob, uid);
            _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(message.Actor):player} has recalled the shuttle.");
        }
        public void OnEmagged(EntityUid uid, CommunicationsConsoleComponent component, ref GotEmaggedEvent args) // DS14-start
        {
            if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
                return;

            if (_emag.CheckFlag(uid, EmagType.Interaction))
                return;

            args.Handled = true;
        }
        private void OnEmagAnnounceMessage(EntityUid uid, CommunicationsConsoleComponent comp, EmagedAnonce message)
        {
            var (hex, color) = GetAnnouncementColor(message.ColorHex);
            var sound = GetAnnouncementSound(message.SoundPath, message.SoundVolume);
            var sender = string.IsNullOrWhiteSpace(message.Announcer)
                ? Loc.GetString("chat-manager-sender-announcement")
                : message.Announcer.Trim();
            var announcementWithSignature = GetAnnouncementWithSignature(message.Announcement, message.Sender);
            if (message.Actor is { Valid: true } mob)
            {
                if (!CanAnnounce(comp))
                {
                    return;
                }

                if (!CanUse(mob, uid))
                {
                    _popupSystem.PopupEntity(Loc.GetString("comms-console-permission-denied"), uid, message.Actor);
                    return;
                }

                var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(uid, mob);
                RaiseLocalEvent(tryGetIdentityShortInfoEvent);
            }

            comp.AnnouncementCooldownRemaining = comp.Delay;
            UpdateCommsConsoleInterface(uid, comp, true);

            var voice = string.Empty;

            if (!message.UseMyTTS && TryComp<TTSComponent>(message.Actor, out var tts))
                voice = tts.VoicePrototypeId;
            _chatSystem.DispatchStationAnnouncement(uid,
                announcementWithSignature,
                sender,
                playDefaultSound: true,
                announcementSound:
                sound,
                colorOverride:
                color,
                voice: voice,
                languageId: message.LanguageId);

            _adminLogger.Add(
                        LogType.Chat,
                        LogImpact.Low,
                        $"{message.Actor} has sent emag announcement " +
                        $"[color={hex}] " +
                        $"[sound={(sound != null ? message.SoundPath : "none")}] " +
                        $"[volume={message.SoundVolume}] " +
                        $"[announcer=\"{message.Announcer}\"] " +
                        $"[sender=\"{message.Sender}\"] " +
                        $": {message.Announcement}"
                    );
        }
        private (string Hex, Color Color) GetAnnouncementColor(string? colorHex)
        {
            var hex = colorHex?.Trim();

            if (string.IsNullOrWhiteSpace(hex))
                hex = "1d8bad";

            if (!hex.StartsWith('#'))
                hex = "#" + hex;

            try
            {
                return (hex, Color.FromHex(hex));
            }
            catch (FormatException)
            {
                return ("#1d8bad", Color.FromHex("#1d8bad"));
            }
        }
        private SoundSpecifier? GetAnnouncementSound(string? soundPath, float soundVolume)
        {
            if (string.IsNullOrWhiteSpace(soundPath))
                return null;

            var path = soundPath.Trim();
            if (!path.StartsWith("/Audio/", StringComparison.OrdinalIgnoreCase) ||
                !HasAnnouncementAudio(path))
            {
                return null;
            }

            var audioParams = AudioParams.Default.WithVolume(soundVolume).AddVolume(-8);
            return new SoundPathSpecifier(path)
            {
                Params = audioParams
            };
        }
        private string GetAnnouncementWithSignature(string announcement, string? signature)
        {
            if (string.IsNullOrWhiteSpace(signature))
                return announcement;

            return $"{announcement}\n{Loc.GetString("comms-console-announcement-sent-by")} {signature.Trim()}";
        }
        private bool HasAnnouncementAudio(string path)
        {
            if (_prototypeManager.HasIndex<AudioMetadataPrototype>(path))
                return true;
            if (!_resourceManager.TryContentFileRead(path, out var stream))
                return false;
            stream.Dispose();
            return true;
        }
        private void PasswordSet(EntityUid uid, CommunicationsConsoleComponent component, ref PasswordSet args)
        {
            if (args.Password == null || component.PassWord != null)
                return;
            if(args.Password.Length > _cfg.GetCVar(CCVars.ChatMaxAnnouncementLength))
                return;
            component.PassWord = args.Password;
        }//DS14-end
    }

    /// <summary>
    /// Raised on announcement
    /// </summary>
    [ByRefEvent]
    public record struct CommunicationConsoleAnnouncementEvent(EntityUid Uid, CommunicationsConsoleComponent Component, string Text, EntityUid? Sender)
    {
        public EntityUid Uid = Uid;
        public CommunicationsConsoleComponent Component = Component;
        public EntityUid? Sender = Sender;
        public string Text = Text;
    }

    /// <summary>
    /// Raised on shuttle call attempt. Can be cancelled
    /// </summary>
    [ByRefEvent]
    public record struct CommunicationConsoleCallShuttleAttemptEvent(EntityUid Uid, CommunicationsConsoleComponent Component, EntityUid? Sender)
    {
        public bool Cancelled = false;
        public EntityUid Uid = Uid;
        public CommunicationsConsoleComponent Component = Component;
        public EntityUid? Sender = Sender;
        public string? Reason;
    }
}
