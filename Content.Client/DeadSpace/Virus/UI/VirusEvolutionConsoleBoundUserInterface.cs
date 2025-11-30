// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

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


        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            _window?.Populate((VirusEvolutionConsoleBoundUserInterfaceState)state);
        }

    }
}
