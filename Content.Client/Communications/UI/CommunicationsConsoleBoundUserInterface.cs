using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Communications;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Content.Shared.DeadSpace.CCCCVars;

namespace Content.Client.Communications.UI
{
    public sealed class CommunicationsConsoleBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        [ViewVariables]
        private CommunicationsConsoleMenu? _menu;
        private EmagCommunicationsInterface? _menuEmag; //DS14
        private string _password = ""; //DS14
        private bool _passwordIsNull = false; //DS14

        public CommunicationsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindow<CommunicationsConsoleMenu>();
            _menu.OnAnnounce += AnnounceButtonPressed;
            _menu.OnBroadcast += BroadcastButtonPressed;
            _menu.OnAlertLevel += AlertLevelSelected;
            _menu.OnEmergencyLevel += EmergencyShuttleButtonPressed;

            _menuEmag = new EmagCommunicationsInterface(); //DS14
            _menuEmag.OnInputPassword += SendPassowrd; //DS14
            _menuEmag.OnOutputMessage += SendEmagAnnoce; //DS14
        }

        public void AlertLevelSelected(string level)
        {
            if (_menu!.AlertLevelSelectable)
            {
                _menu.CurrentLevel = level;
                SendMessage(new CommunicationsConsoleSelectAlertLevelMessage(level));
            }
        }

        public void EmergencyShuttleButtonPressed()
        {
            if (_menu!.CountdownStarted)
                RecallShuttle();
            else
                CallShuttle();
        }

        public void AnnounceButtonPressed(string message)
        {
            var maxLength = _cfg.GetCVar(CCVars.ChatMaxAnnouncementLength);
            var msg = SharedChatSystem.SanitizeAnnouncement(message, maxLength);
            SendMessage(new CommunicationsConsoleAnnounceMessage(msg));
        }

        public void BroadcastButtonPressed(string message)
        {
            SendMessage(new CommunicationsConsoleBroadcastMessage(SharedChatSystem.SanitizeAnnouncement(message, _cfg.GetCVar(CCCCVars.MaxBroadcastLength)))); //DS14
        }

        public void CallShuttle()
        {
            SendMessage(new CommunicationsConsoleCallEmergencyShuttleMessage());
        }

        public void RecallShuttle()
        {
            SendMessage(new CommunicationsConsoleRecallEmergencyShuttleMessage());
        }
        public void SendPassowrd(string password) //DS14-start
        {
            _password = password;
            SendMessage(new PasswordSet(password));
        }
        public void SendEmagAnnoce(EmagedAnonce message)
        {
            if (_passwordIsNull)
            {
                message.Password = _password;
            }
            SendMessage(message);
        } //DS14-end

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not CommunicationsConsoleInterfaceState commsState)
                return;
            _password = commsState.Password ?? ""; //DS14
            if (_menu != null && !commsState.RigtAnswer) //DS14
            {
                _menu.CanAnnounce = commsState.CanAnnounce;
                _menu.CanBroadcast = commsState.CanBroadcast;
                _menu.CanCall = commsState.CanCall;
                _menu.CountdownStarted = commsState.CountdownStarted;
                _menu.AlertLevelSelectable = commsState.AlertLevels != null && !float.IsNaN(commsState.CurrentAlertDelay) && commsState.CurrentAlertDelay <= 0;
                _menu.CurrentLevel = commsState.CurrentAlert;
                _menu.CountdownEnd = commsState.ExpectedCountdownEnd;

                _menu.UpdateCountdown();
                _menu.UpdateAlertLevels(commsState.AlertLevels, _menu.CurrentLevel);
                _menu.AlertLevelButton.Disabled = !_menu.AlertLevelSelectable;
                _menu.EmergencyShuttleButton.Disabled = !_menu.CanCall;
                _menu.AnnounceButton.Disabled = !_menu.CanAnnounce;
                _menu.BroadcastButton.Disabled = !_menu.CanBroadcast;
            }
            if (_menuEmag != null && (commsState.RigtAnswer || commsState.PassWordIsNull)) //DS14-start
            {
                if (_menu != null)
                {
                    _menu.Close();
                }
                if (!_menuEmag.IsOpen)
                {
                    _menuEmag.OpenCentered();
                    _menuEmag.MaxPassWordLenght =_cfg.GetCVar(CCVars.ChatMaxAnnouncementLength);
                    _menuEmag.HavePassword = commsState.RigtAnswer;
                    _passwordIsNull = commsState.PassWordIsNull;
                }
            } //DS14-end
        }
    }
}
