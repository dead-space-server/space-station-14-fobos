using System.IO; // DS14
using Content.Shared.PDA;
using Content.Shared.PDA.Ringer;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;
using Robust.Shared.Utility; // DS14

namespace Content.Client.PDA.Ringer
{
    [UsedImplicitly]
    public sealed class RingerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
    {
        // DS14-Start
        [Dependency] private readonly IFileDialogManager _dialogManager = default!;
        private bool _isMidiFileDialogOpen;
        // DS14-End

        [ViewVariables]
        private RingtoneMenu? _menu;

        protected override void Open()
        {
            base.Open();
            IoCManager.InjectDependencies(this); // DS14
            _menu = this.CreateWindow<RingtoneMenu>();
            _menu.OpenToLeft();

            _menu.TestRingtoneButtonPressed += OnTestRingtoneButtonPressed;
            _menu.SetRingtoneButtonPressed += OnSetRingtoneButtonPressed;
            // DS14-Start
            _menu.LoadMidiRingtoneButtonPressed += OnLoadMidiRingtoneButtonPressed;
            // DS14-End

            Update();
        }

        private bool TryGetRingtone(out Note[] ringtone)
        {
            if (_menu == null)
            {
                ringtone = Array.Empty<Note>();
                return false;
            }

            ringtone = new Note[_menu.RingerNoteInputs.Length];

            for (int i = 0; i < _menu.RingerNoteInputs.Length; i++)
            {
                if (!Enum.TryParse<Note>(_menu.RingerNoteInputs[i].Text.Replace("#", "sharp"), false, out var note))
                    return false;
                ringtone[i] = note;
            }

            return true;
        }

        public override void Update()
        {
            base.Update();

            if (_menu == null)
                return;

            if (!EntMan.TryGetComponent(Owner, out RingerComponent? ringer))
                return;

            for (var i = 0; i < _menu.RingerNoteInputs.Length; i++)
            {
                var note = ringer.Ringtone[i].ToString();

                if (!RingtoneMenu.IsNote(note))
                    continue;

                _menu.PreviousNoteInputs[i] = note.Replace("sharp", "#");
                _menu.RingerNoteInputs[i].Text = _menu.PreviousNoteInputs[i];
            }

            _menu.TestRingerButton.Disabled = ringer.Active;

            // DS14-Start
            _menu.MidiRingtoneStatus.Text = ringer.MidiRingtoneData != null && ringer.MidiRingtoneData.Length > 0
                ? "✓"
                : "";
            // DS14-End
        }

        private void OnTestRingtoneButtonPressed()
        {
            if (_menu is null)
                return;

            SendPredictedMessage(new RingerPlayRingtoneMessage());

            // We disable it instantly to remove the delay before the client receives the next compstate
            // Makes the UI feel responsive, will be re-enabled by ringer.Active once it gets an update.
            _menu.TestRingerButton.Disabled = true;
        }

        private void OnSetRingtoneButtonPressed()
        {
            if (_menu is null)
                return;

            if (!TryGetRingtone(out var ringtone))
                return;

            SendPredictedMessage(new RingerSetRingtoneMessage(ringtone));
            _menu.SetRingerButton.Disabled = true;

            Timer.Spawn(333,
                () =>
                {
                    if (_menu is { Disposed: false, SetRingerButton: { Disposed: false } ringer} )
                        ringer.Disabled = false;
                });
        }

        // DS14-Start
        private async void OnLoadMidiRingtoneButtonPressed()
        {
            if (_menu is null || _isMidiFileDialogOpen)
                return;

            var filters = new FileDialogFilters(new FileDialogFilters.Group("mid", "midi"));

            _isMidiFileDialogOpen = true;
            await using var file = await _dialogManager.OpenFile(filters, FileAccess.Read);
            _isMidiFileDialogOpen = false;

            if (file == null)
                return;

            var midiData = file.CopyToArray();
            if (midiData.Length == 0)
                return;

            SendMessage(new RingerSetMidiRingtoneMessage(midiData));
        }
        // DS14-End
    }
}
