// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Shared.Virus;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.Virus.UI
{
    [UsedImplicitly]
    public sealed class VirusEvolutionConsoleBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VirusEvolutionConsoleWindow? _window;

        public VirusEvolutionConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        { }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<VirusEvolutionConsoleWindow>();

            _window.BuyBodyButton.OnPressed += _ =>
                SendMessage(new EvolutionConsoleUiButtonPressedMessage(EvolutionConsoleUiButton.EvolutionBody, newBodie: GenSelectedBody()));

            _window.BuySymptomButton.OnPressed += _ =>
                SendMessage(new EvolutionConsoleUiButtonPressedMessage(EvolutionConsoleUiButton.EvolutionSymptom, newSymptom: GenSelectedSymptom()));


        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            _window?.Populate((VirusEvolutionConsoleBoundUserInterfaceState)state);
        }

        private string? GenSelectedSymptom()
        {
            if (_window == null)
                return null;

            var symptom = _window.ActiveSymptomsList.GetSelected().FirstOrDefault()?.Text;

            if (symptom == null)
                return null;

            return symptom.Split('-')[0];
        }

        private string? GenSelectedBody()
        {
            if (_window == null)
                return null;

            var body = _window.BodyWhitelistList.GetSelected().FirstOrDefault()?.Text;

            if (body == null)
                return null;

            return body.Split('-')[0];
        }

    }
}
