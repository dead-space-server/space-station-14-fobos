using Content.Client.Eui;
using Content.Shared.DeadSpace.ItemTransfer;
using Robust.Client.Graphics;

namespace Content.Client.DeadSpace.ItemTransfer;

public sealed class ItemTransferOfferSystem : EntitySystem
{
    private YesNoWindow? _window;
    private int? _requestId;
    private bool _serverClosing;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ItemTransferOfferMessage>(OnOffer);
        SubscribeNetworkEvent<ItemTransferOfferClosedMessage>(OnOfferClosed);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CloseWindow();
    }

    private void OnOffer(ItemTransferOfferMessage message, EntitySessionEventArgs args)
    {
        CloseWindow();

        _requestId = message.RequestId;
        _window = new YesNoWindow(
            Loc.GetString("item-transfer-window-title"),
            Loc.GetString("item-transfer-window-message", ("user", message.UserName), ("item", message.ItemName)));

        _window.YesButton.Text = Loc.GetString("item-transfer-window-accept");
        _window.NoButton.Text = Loc.GetString("item-transfer-window-decline");

        _window.YesButton.OnPressed += _ => SendAnswer(true);
        _window.NoButton.OnPressed += _ => SendAnswer(false);
        _window.OnClose += OnWindowClosed;

        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    private void OnOfferClosed(ItemTransferOfferClosedMessage message, EntitySessionEventArgs args)
    {
        if (_requestId != message.RequestId)
            return;

        CloseWindow();
    }

    private void OnWindowClosed()
    {
        if (_serverClosing)
            return;

        SendAnswer(false);
    }

    private void SendAnswer(bool accepted)
    {
        if (_requestId is not { } requestId)
            return;

        RaiseNetworkEvent(new ItemTransferAnswerMessage(requestId, accepted));
        CloseWindow();
    }

    private void CloseWindow()
    {
        _requestId = null;

        if (_window == null)
            return;

        _serverClosing = true;
        _window.Close();
        _serverClosing = false;
        _window = null;
    }
}
