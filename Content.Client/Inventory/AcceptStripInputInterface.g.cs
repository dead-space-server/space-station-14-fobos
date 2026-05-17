using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
namespace Content.Client.Inventory
{
    partial class AcceptStipInputInterface
    {
        private global::Robust.Client.UserInterface.Controls.BoxContainer ActiveCallControlsContainer => this.FindControl<global::Robust.Client.UserInterface.Controls.BoxContainer>("ActiveCallControlsContainer");
        private global::Robust.Client.UserInterface.Controls.BoxContainer InsertnerIdContainer => this.FindControl<global::Robust.Client.UserInterface.Controls.BoxContainer>("InsertnerIdContainer");
        private global::Robust.Client.UserInterface.Controls.Label MessegeText => this.FindControl<global::Robust.Client.UserInterface.Controls.Label>("MessegeText");
        public global::Robust.Client.UserInterface.Controls.Button AnswerCallButton => this.FindControl<global::Robust.Client.UserInterface.Controls.Button>("AnswerCallButton");
        public global::Robust.Client.UserInterface.Controls.Button EndCallButton => this.FindControl<global::Robust.Client.UserInterface.Controls.Button>("EndCallButton");
    }
}
