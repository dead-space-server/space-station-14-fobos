using Content.Client.Eui;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.Administration.UI
{
    public sealed class AdminAnnounceEui : BaseEui
    {
        private readonly AdminAnnounceWindow _window;

        public AdminAnnounceEui()
        {
            _window = new AdminAnnounceWindow();
            _window.OnClose += () => SendMessage(new CloseEuiMessage());
            _window.AnnounceButton.OnPressed += AnnounceButtonOnOnPressed;
        }

        // DS14-announce-start
        private void AnnounceButtonOnOnPressed(BaseButton.ButtonEventArgs obj)
        {
            var target = _window.SelectedTarget;

            SendMessage(new AdminAnnounceEuiMsg.DoAnnounce
            {
                Announcement = Rope.Collapse(_window.Announcement.TextRope),
                Announcer = _window.Announcer.Text,
                AnnounceType = target.Type,
                CloseAfter = !_window.KeepWindowOpen.Pressed,
                Voice = (string) (_window.VoiceSelector.SelectedMetadata ?? ""),
                LanguageId = (string)(_window.LanguageSelector.SelectedMetadata ?? ""), // DS14-Languages
                EnableTTS = _window.EnableTTS.Pressed,
                CustomTTS = _window.CustomTTS.Pressed,
                TargetGrid = target.Grid,
                ColorHex = _window.ColorHexText,
                SoundPath = _window.SoundPathText,
                SoundVolume = _window.SoundVolumeValue,
                Sender = _window.SenderText,
            });
        }

        public override void HandleState(EuiStateBase state)
        {
            if (state is AdminAnnounceEuiState announceState)
                _window.SetAnnouncementTargets(announceState.Targets);
        }
        // DS14-announce-end

        public override void Opened()
        {
            _window.OpenCentered();
        }

        public override void Closed()
        {
            _window.Close();
        }
    }
}
