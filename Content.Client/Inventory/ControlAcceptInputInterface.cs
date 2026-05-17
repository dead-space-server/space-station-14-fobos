//Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Strip.Components;

namespace Content.Client.Inventory;
public sealed partial class ControlAcceptStripInt : EntitySystem
{
    private AcceptStripInputInterface? _menu;
    private int? _requestId;

    public override void Initialize()
    {
        SubscribeNetworkEvent<StartStripInsertInventoryMessage>(Open);
        SubscribeNetworkEvent<EndStripInsertInventoryMessage>(CloseFunction);
    }

    private void Open(StartStripInsertInventoryMessage message)
    {
        if (_menu != null)
            _menu.Close();

        _requestId = message.RequestId;
        _menu = new AcceptStripInputInterface(message);
        _menu.OpenCenteredLeft();
        _menu.Title = Loc.GetString("strippable-bound-user-interface-inserting-menu-title");
        _menu.Answered += answer => AnswerFunction(answer, message.RequestId);
    }

    public void AnswerFunction(bool answer, int requestId)
    {
        if (_requestId != requestId)
            return;

        RaiseNetworkEvent(new AnswerStripInsertInventoryMessage(requestId, answer));
        _menu?.Close();
        _menu = null;
        _requestId = null;
    }

    public void CloseFunction(EndStripInsertInventoryMessage message)
    {
        if (_requestId != message.RequestId)
            return;

        if (_menu != null)
            _menu.Close();

        _menu = null;
        _requestId = null;
    }
}
