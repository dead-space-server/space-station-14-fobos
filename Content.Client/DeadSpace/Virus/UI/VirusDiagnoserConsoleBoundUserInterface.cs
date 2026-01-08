// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Content.Shared.DeadSpace.Virus;
using System.Linq;

namespace Content.Client.DeadSpace.Virus.UI
{
    [UsedImplicitly]
    public sealed class VirusDiagnoserConsoleBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VirusDiagnoserConsoleWindow? _window;

        public VirusDiagnoserConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        { }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<VirusDiagnoserConsoleWindow>();

            _window.ScanVirusButton.OnPressed += _ =>
                SendMessage(new UiButtonPressedMessage(UiButton.ScanVirus, null));

            _window.StartAnalysButton.OnPressed += _ =>
                SendMessage(new UiButtonPressedMessage(UiButton.StartAnalys, null));

            _window.GenerateVirusButton.OnPressed += _ =>
            {
                var strainName = GenSelectedRecord();
                SendMessage(new UiButtonPressedMessage(UiButton.GenerateVirus, strainName));
            };

            _window.PrintReportButton.OnPressed += _ =>
            {
                var strainName = GenSelectedRecord();
                SendMessage(new UiButtonPressedMessage(UiButton.PrintReport, strainName));
            };

            _window.DeleteStrainButton.OnPressed += _ =>
            {
                var strainName = GenSelectedRecord();
                SendMessage(new UiButtonPressedMessage(UiButton.DeleteData, strainName));
            };
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            _window?.Populate((VirusDiagnoserConsoleBoundUserInterfaceState)state);
        }

        private string? GenSelectedRecord()
        {
            if (_window == null)
                return null;

            var strainName = _window.StrainList.GetSelected().FirstOrDefault()?.Text;

            if (strainName == null)
                return null;

            return strainName.Split('-')[0];
        }
    }
}
