// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Shared.Body.Prototypes;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Content.Shared.DeadSpace.Virus;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.DeadSpace.Virus.UI
{
    [UsedImplicitly]
    public sealed class VirusEvolutionConsoleBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VirusEvolutionConsoleWindow? _window;

        public VirusEvolutionConsoleBoundUserInterface(EntityUid owner, Enum uiKey)
            : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<VirusEvolutionConsoleWindow>();

            // Покупка симптома
            _window.BuySymptomButton.OnPressed += _ =>
                SendMessage(new EvolutionConsoleUiButtonPressedMessage(
                    EvolutionConsoleUiButton.EvolutionSymptom,
                    symptom: GenSelectedAvailableSymptom()
                ));

            // Покупка тела
            _window.BuyBodyButton.OnPressed += _ =>
                SendMessage(new EvolutionConsoleUiButtonPressedMessage(
                    EvolutionConsoleUiButton.EvolutionBody,
                    body: GenSelectedAvailableBody()
                ));

            // Удаление симптома
            _window.DeleteSymptomButton.OnPressed += _ =>
                SendMessage(new EvolutionConsoleUiButtonPressedMessage(
                    EvolutionConsoleUiButton.DeleteSymptom,
                    symptom: GenSelectedActiveSymptom()
                ));

            // Удаление тела
            _window.DeleteBodyButton.OnPressed += _ =>
                SendMessage(new EvolutionConsoleUiButtonPressedMessage(
                    EvolutionConsoleUiButton.DeleteBody,
                    body: GenSelectedActiveBody()
                ));
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            _window?.Populate((VirusEvolutionConsoleBoundUserInterfaceState)state);
        }

        private string? GenSelectedAvailableSymptom()
        {
            if (_window == null)
                return null;

            var item = _window.AvailableSymptomsList.GetSelected().FirstOrDefault();
            return item?.Metadata as string;
        }

        private string? GenSelectedAvailableBody()
        {
            if (_window == null)
                return null;

            var item = _window.AvailableBodiesList.GetSelected().FirstOrDefault();
            return item?.Metadata as string;
        }

        private string? GenSelectedActiveSymptom()
        {
            if (_window == null)
                return null;

            var item = _window.ActiveSymptomsList.GetSelected().FirstOrDefault();
            if (item?.Metadata is ProtoId<VirusSymptomPrototype> id)
                return id.Id;

            return null;
        }

        private string? GenSelectedActiveBody()
        {
            if (_window == null)
                return null;

            var item = _window.ActiveBodiesList.GetSelected().FirstOrDefault();
            if (item?.Metadata is ProtoId<BodyPrototype> id)
                return id.Id;

            return null;
        }
    }
}
