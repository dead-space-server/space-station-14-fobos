using Content.Server.EUI;
using Content.Shared.DeadSpace.UI;
using Content.Shared.Eui;

namespace Content.Server.DeadSpace.ERT
{
    public sealed class YesNoEui : BaseEui
    {
        private readonly EntityUid _target;
        private readonly ResponceErtOnAllowedStateSystem _system;
        private readonly string _title;
        private readonly string _text;

        public YesNoEui(EntityUid target, ResponceErtOnAllowedStateSystem system, string? title = null, string? text = null)
        {
            _target = target;
            _system = system;
            _title = title ?? "Question";
            _text = text ?? "Proceed?";
        }

        public override void Opened()
        {
            // Send initial state to client.
            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            return new YesNoEuiState(_title, _text);
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            if (msg is not YesNoChoiceMessage choice)
            {
                Close();
                return;
            }

            var accepted = choice.Button == YesNoUiButton.Yes;
            _system.HandleYesNoResponse(_target, Player, accepted);
            Close();
        }
    }
}
