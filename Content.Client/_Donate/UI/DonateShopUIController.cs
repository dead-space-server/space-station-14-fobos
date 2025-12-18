// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared._Donate;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Donate.UI;

public sealed class DonateShopUIController : UIController
{
    [Dependency] private readonly IEntityManager _manager = default!;

    private DonateShopWindow? _donateShopWindow;

    private MenuButton? DonateButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.DonateButton;

    public void UnloadButton()
    {
        if (DonateButton == null)
        {
            return;
        }

        DonateButton.Pressed = false;
        DonateButton.OnPressed -= OnPressed;
    }

    public void LoadButton()
    {
        if (DonateButton == null)
        {
            return;
        }

        DonateButton.OnPressed += OnPressed;
    }

    private void OnPressed(BaseButton.ButtonEventArgs obj)
    {
        ToggleWindow();
    }

    public void UpdateWindowState(DonateShopState state)
    {
        if (_donateShopWindow == null)
            return;

        _donateShopWindow.ApplyState(state);
    }

    public void UpdateEnergyShopState(EnergyShopState state)
    {
        if (_donateShopWindow == null)
            return;

        _donateShopWindow.ApplyEnergyShopState(state);
    }

    public void HandlePurchaseResult(PurchaseResult result)
    {
        if (_donateShopWindow == null)
            return;

        _donateShopWindow.ShowPurchaseResult(result);
    }

    public void ToggleWindow()
    {
        if (_donateShopWindow == null)
        {
            _donateShopWindow = new DonateShopWindow();
            _donateShopWindow.OnClose += () =>
            {
                _donateShopWindow = null;

                if (DonateButton != null)
                    DonateButton.Pressed = false;
            };
            _donateShopWindow.OpenCentered();
            _manager.EntityNetManager.SendSystemNetworkMessage(new RequestUpdateDonateShop());
            return;
        }

        if (_donateShopWindow.IsOpen)
        {
            _donateShopWindow.Close();
        }
        else
        {
            _donateShopWindow.OpenCentered();
            _manager.EntityNetManager.SendSystemNetworkMessage(new RequestUpdateDonateShop());
        }
    }
}
