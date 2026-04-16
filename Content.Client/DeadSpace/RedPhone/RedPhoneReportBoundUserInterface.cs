// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.RedPhone;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.RedPhone;

[UsedImplicitly]
public sealed class RedPhoneReportBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private RedPhoneReportWindow? _window;

    public RedPhoneReportBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<RedPhoneReportWindow>();
        _window.Submit += message => SendMessage(new RedPhoneSubmitReportMessage(message));
        _window.MessageInput.GrabKeyboardFocus();
    }
}
