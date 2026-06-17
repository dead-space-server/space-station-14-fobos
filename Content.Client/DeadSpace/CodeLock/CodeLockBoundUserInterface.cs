using Content.Shared.DeadSpace.CodeLock;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.CodeLock
{
    [UsedImplicitly]
    public sealed class CodeLockBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private CodeLockMenu? _menu;

        public CodeLockBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindow<CodeLockMenu>();

            _menu.OnKeypadButtonPressed += i =>
            {
                SendMessage(new CodeLockKeypadMessage(i));
            };
            _menu.OnEnterButtonPressed += () =>
            {
                SendMessage(new CodeLockKeypadEnterMessage());
            };
            _menu.OnClearButtonPressed += () =>
            {
                SendMessage(new CodeLockKeypadClearMessage());
            };
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (_menu == null)
                return;

            switch (state)
            {
                case CodeLockUiState msg:
                    _menu.UpdateState(msg);
                    break;
            }
        }
    }
}
