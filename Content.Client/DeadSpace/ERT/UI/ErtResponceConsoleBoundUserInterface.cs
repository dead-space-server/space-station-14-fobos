// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Content.Shared.DeadSpace.ERT;

namespace Content.Client.DeadSpace.ERT.UI
{
    [UsedImplicitly]
    public sealed class ErtResponceConsoleBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private ErtResponceConsoleWindow? _window;

        public ErtResponceConsoleBoundUserInterface(EntityUid owner, Enum uiKey)
            : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<ErtResponceConsoleWindow>();

            _window.ResponceTeamButton.OnPressed += _ =>
                SendMessage(new ErtResponceConsoleUiButtonPressedMessage(
                    ErtResponceConsoleUiButton.ResponceErt,
                    team: GenSelectedAvailableTeam()
                ));

        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            _window?.Populate((ErtResponceConsoleBoundUserInterfaceState)state);
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
