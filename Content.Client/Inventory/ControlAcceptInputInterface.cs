//Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Content.Shared.Strip.Components;

namespace Content.Client.Inventory;
public sealed partial class ControlAcceptStripInt : EntitySystem
{
    private AcceptStipInputInterface? _menu;
    public override void Initialize()
    {
        SubscribeNetworkEvent<StartStripInsertInventoryMessage>(Open);
        SubscribeNetworkEvent<EndStripInsertInventoryMessage>(CloseFunction);
    }
    private void Open(StartStripInsertInventoryMessage message)
    {
        if (_menu != null)
            _menu.Close();
        _menu = new AcceptStipInputInterface(message);
        _menu.OpenCenteredLeft();
        _menu.Title = Loc.GetString("strippable-bound-user-interface-inserting-menu-title");
        // Assign button actions
        _menu.AnswerCallButton.OnPressed += args => { AnswerFunction(true, message.WhoAnswer, _menu); };
        _menu.EndCallButton.OnPressed += args => { AnswerFunction(false, message.WhoAnswer, _menu); };
    }

    public void AnswerFunction(bool answer, int eUid, AcceptStipInputInterface menu)
    {
        RaiseNetworkEvent(new AnswerStripInsertInventoryMessage(eUid, answer));
        menu.Close();
    }
    public void CloseFunction(EndStripInsertInventoryMessage message)
    {
        if (_menu != null)
            _menu.Close();
    }
}
