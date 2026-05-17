// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
namespace Content.Client.Inventory
{
    partial class AcceptStipInputInterface
    {
        private global::Robust.Client.UserInterface.Controls.BoxContainer ActiveCallControlsContainer => this.FindControl<global::Robust.Client.UserInterface.Controls.BoxContainer>("ActiveCallControlsContainer");
        private global::Robust.Client.UserInterface.Controls.BoxContainer InsertnerIdContainer => this.FindControl<global::Robust.Client.UserInterface.Controls.BoxContainer>("InsertnerIdContainer");
        private global::Robust.Client.UserInterface.Controls.Label MessageText => this.FindControl<global::Robust.Client.UserInterface.Controls.Label>("MessageText");
        public global::Robust.Client.UserInterface.Controls.Button AnswerCallButton => this.FindControl<global::Robust.Client.UserInterface.Controls.Button>("AnswerCallButton");
        public global::Robust.Client.UserInterface.Controls.Button EndCallButton => this.FindControl<global::Robust.Client.UserInterface.Controls.Button>("EndCallButton");
    }
}
