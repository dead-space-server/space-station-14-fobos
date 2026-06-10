// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Content.Shared.DeadSpace.ERT;

namespace Content.Client.DeadSpace.ERT.UI
{
    [UsedImplicitly]
    public sealed class ErtResponseConsoleBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private ErtResponseConsoleWindow? _window;

        public ErtResponseConsoleBoundUserInterface(EntityUid owner, Enum uiKey)
            : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<ErtResponseConsoleWindow>();

            _window.ResponseTeamButton.OnPressed += _ =>
                SendMessage(new ErtResponseConsoleUiButtonPressedMessage(
                    ErtResponseConsoleUiButton.ResponseErt,
                    team: GenSelectedAvailableTeam(),
                    callReason: GetCallReason()
                ));

        }

        private string? GetCallReason()
        {
            if (_window == null)
                return null;

            var reason = _window.CallReasonEdit.Text;
            return string.IsNullOrWhiteSpace(reason) ? null : reason;
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            _window?.Populate((ErtResponseConsoleBoundUserInterfaceState)state);
        }

        private string? GenSelectedAvailableTeam()
        {
            if (_window == null)
                return null;

            var item = _window.AvailableTeamsList.GetSelected().FirstOrDefault();
            return item?.Metadata as string;
        }


    }
}
