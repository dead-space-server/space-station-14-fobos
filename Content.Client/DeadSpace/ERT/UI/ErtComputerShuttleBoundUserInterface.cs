// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Content.Shared.DeadSpace.ERT;

namespace Content.Client.DeadSpace.ERT.UI
{
    [UsedImplicitly]
    public sealed class ErtComputerShuttleBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private ErtComputerShuttleWindow? _window;

        public ErtComputerShuttleBoundUserInterface(EntityUid owner, Enum uiKey)
            : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<ErtComputerShuttleWindow>();

            _window.StartEvacuationButton.OnPressed += _ =>
                SendMessage(new ErtComputerShuttleUiButtonPressedMessage(
                    ErtComputerShuttleUiButton.Evacuation
                ));

            _window.CancelEvacuationButton.OnPressed += _ =>
                SendMessage(new ErtComputerShuttleUiButtonPressedMessage(
                    ErtComputerShuttleUiButton.CancelEvacuation
                ));

        }



    }
}
