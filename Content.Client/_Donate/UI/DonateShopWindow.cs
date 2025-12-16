// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using System.Numerics;
using Content.Client._Donate.Emerald;
using Content.Shared._Donate;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Donate.UI;

public sealed class DonateShopWindow : EmeraldDefaultWindow
{
    [Dependency] private readonly IUriOpener _url = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;

    private enum Tab
    {
        Profile,
        Inventory,
        Shop
    }

    private DonateShopState? _state;
    private EnergyShopState? _energyShopState;
    private string? _currentCategory;
    private string? _currentShopCategory;
    private List<string> _categories = new();
    private List<string> _shopCategories = new();

    private EmeraldLevelBar _levelBar = default!;
    private EmeraldBatteryDisplay _batteryDisplay = default!;
    private EmeraldCrystalDisplay _crystalDisplay = default!;
    private EmeraldShopButton _shopButton = default!;

    private Control _topPanel = default!;
    private WrapContainer _topPanelWrap = default!;
    private Control _tabsContainer = default!;

    private EmeraldButton _profileTabButton = default!;
    private EmeraldButton _inventoryTabButton = default!;
    private EmeraldButton _shopTabButton = default!;

    private BoxContainer _contentContainer = default!;
    private Control _profileContent = default!;
    private Control _inventoryContent = default!;
    private Control _shopContent = default!;

    private BoxContainer _profilePanel = default!;
    private BoxContainer _categoryTabsContainer = default!;
    private BoxContainer _categoryItemsPanel = default!;
    private EmeraldSearchBox _searchBox = default!;
    private GridContainer? _itemsGrid;
    private GridContainer? _perksGrid;
    private string _searchQuery = "";

    private BoxContainer _shopCategoryTabsContainer = default!;
    private BoxContainer _shopItemsPanel = default!;
    private EmeraldSearchBox _shopSearchBox = default!;
    private GridContainer? _shopItemsGrid;
    private string _shopSearchQuery = "";
    private bool _shopLoading;
    private bool _isPurchasing;

    public DonateShopWindow()
    {
        IoCManager.InjectDependencies(this);

        Title = "MK TERMINAL";
        MinSize = new Vector2(816, 817);
        SetSize = new Vector2(816, 817);

        BuildUI();
        ShowLoading();
    }

    private void BuildUI()
    {
        var mainContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            SeparationOverride = 0
        };

        _topPanel = BuildTopPanel();
        mainContainer.AddChild(_topPanel);

        mainContainer.AddChild(new Control { MinHeight = 10 });

        _tabsContainer = BuildTabsContainer();
        mainContainer.AddChild(_tabsContainer);

        mainContainer.AddChild(new Control { MinHeight = 10 });

        _contentContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true
        };
        mainContainer.AddChild(_contentContainer);

        _profileContent = BuildProfileContent();
        _inventoryContent = BuildInventoryContent();
        _shopContent = BuildShopContent();

        AddContent(mainContainer);

        SwitchTab(Tab.Profile);
    }

    private void ShowLoading()
    {
        _profilePanel.RemoveAllChildren();

        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            SeparationOverride = 12
        };

        var loadingLabel = new EmeraldLabel
        {
            Text = "ИДЁТ ЗАГРУЗКА...",
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4)
        };
        container.AddChild(loadingLabel);

        var waitLabel = new EmeraldLabel
        {
            Text = "Подождите, пожалуйста",
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center
        };
        container.AddChild(waitLabel);

        _profilePanel.AddChild(container);
    }

    private Control BuildTopPanel()
    {
        var panel = new EmeraldPanel
        {
            HorizontalExpand = true,
            Visible = false,
            Margin = new Thickness(2, 0, 0, 0)
        };

        var mainContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            Margin = new Thickness(8, 6),
            SeparationOverride = 6
        };

        _topPanelWrap = new WrapContainer
        {
            HorizontalExpand = true,
            LayoutAxis = Axis.Horizontal,
            SeparationOverride = 8,
            CrossSeparationOverride = 6
        };

        _levelBar = new EmeraldLevelBar
        {
            MinWidth = 200,
            HorizontalExpand = true,
            Level = 1,
            Experience = 0,
            RequiredExp = 100,
            ToNextLevel = 100,
            Progress = 0f,
            Margin = new Thickness(0, 2)
        };
        _topPanelWrap.AddChild(_levelBar);

        _batteryDisplay = new EmeraldBatteryDisplay
        {
            Amount = 0,
            IconTexturePath = "/Textures/Interface/battery.png",
            MinWidth = 140,
            Margin = new Thickness(0, 2)
        };
        _topPanelWrap.AddChild(_batteryDisplay);

        _crystalDisplay = new EmeraldCrystalDisplay
        {
            Amount = 0,
            IconTexturePath = "/Textures/Interface/crystal.png",
            MinWidth = 140,
            Margin = new Thickness(0, 2)
        };
        _topPanelWrap.AddChild(_crystalDisplay);

        mainContainer.AddChild(_topPanelWrap);

        _shopButton = new EmeraldShopButton
        {
            Text = "МАГАЗИН",
            Margin = new Thickness(0, 2),
            HorizontalAlignment = HAlignment.Center
        };
        _shopButton.OnPressed += () =>
        {
            _url.OpenUri("https://deadspace14.net");
        };
        mainContainer.AddChild(_shopButton);

        panel.AddChild(mainContainer);
        return panel;
    }

    private Control BuildTabsContainer()
    {
        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Visible = false,
            SeparationOverride = 10,
            Margin = new Thickness(2, 0, 2, 0)
        };

        _profileTabButton = new EmeraldButton
        {
            Text = "ПРОФИЛЬ",
            HorizontalExpand = true
        };
        _profileTabButton.OnPressed += () => SwitchTab(Tab.Profile);
        container.AddChild(_profileTabButton);

        _inventoryTabButton = new EmeraldButton
        {
            Text = "ИНВЕНТАРЬ",
            HorizontalExpand = true
        };
        _inventoryTabButton.OnPressed += () => SwitchTab(Tab.Inventory);
        container.AddChild(_inventoryTabButton);

        _shopTabButton = new EmeraldButton
        {
            Text = "ENERGY SHOP",
            HorizontalExpand = true
        };
        _shopTabButton.OnPressed += () => SwitchTab(Tab.Shop);
        container.AddChild(_shopTabButton);

        return container;
    }

    private Control BuildProfileContent()
    {
        var scrollContainer = new EmeraldScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true
        };

        _profilePanel = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            SeparationOverride = 12,
            Margin = new Thickness(2)
        };

        scrollContainer.SetContent(_profilePanel);
        return scrollContainer;
    }

    private Control BuildInventoryContent()
    {
        var mainContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            SeparationOverride = 10
        };

        _searchBox = new EmeraldSearchBox
        {
            HorizontalExpand = true,
            Placeholder = "ПОИСК...",
            Margin = new Thickness(2, 0, 2, 0)
        };
        _searchBox.OnTextChanged += text =>
        {
            _searchQuery = text.ToLower();
            if (_state != null && _currentCategory != null)
                SwitchCategory(_currentCategory);
        };
        mainContainer.AddChild(_searchBox);

        _categoryTabsContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            SeparationOverride = 5,
            Margin = new Thickness(2, 0, 2, 0)
        };
        mainContainer.AddChild(_categoryTabsContainer);

        var scrollContainer = new EmeraldScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true
        };

        _categoryItemsPanel = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SeparationOverride = 12,
            Margin = new Thickness(2)
        };

        scrollContainer.SetContent(_categoryItemsPanel);
        mainContainer.AddChild(scrollContainer);

        return mainContainer;
    }

    private Control BuildShopContent()
    {
        var mainContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            SeparationOverride = 10
        };

        _shopSearchBox = new EmeraldSearchBox
        {
            HorizontalExpand = true,
            Placeholder = "ПОИСК В МАГАЗИНЕ...",
            Margin = new Thickness(2, 0, 2, 0)
        };
        _shopSearchBox.OnTextChanged += text =>
        {
            _shopSearchQuery = text.ToLower();
            if (_energyShopState != null && _currentShopCategory != null)
                SwitchShopCategory(_currentShopCategory);
        };
        mainContainer.AddChild(_shopSearchBox);

        _shopCategoryTabsContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            SeparationOverride = 5,
            Margin = new Thickness(2, 0, 2, 0)
        };
        mainContainer.AddChild(_shopCategoryTabsContainer);

        var scrollContainer = new EmeraldScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true
        };

        _shopItemsPanel = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SeparationOverride = 12,
            Margin = new Thickness(2)
        };

        scrollContainer.SetContent(_shopItemsPanel);
        mainContainer.AddChild(scrollContainer);

        return mainContainer;
    }

    private void SwitchTab(Tab tab)
    {
        _profileTabButton.IsActive = tab == Tab.Profile;
        _inventoryTabButton.IsActive = tab == Tab.Inventory;
        _shopTabButton.IsActive = tab == Tab.Shop;

        _contentContainer.RemoveAllChildren();

        switch (tab)
        {
            case Tab.Profile:
                _contentContainer.AddChild(_profileContent);
                break;
            case Tab.Inventory:
                _contentContainer.AddChild(_inventoryContent);
                break;
            case Tab.Shop:
                _contentContainer.AddChild(_shopContent);
                if (_energyShopState == null && !_shopLoading)
                {
                    _shopLoading = true;
                    ShowShopLoading();
                    _entManager.EntityNetManager.SendSystemNetworkMessage(new RequestEnergyShopItems());
                }
                break;
        }
    }

    private void ShowShopLoading()
    {
        _shopItemsPanel.RemoveAllChildren();

        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            SeparationOverride = 12
        };

        var loadingLabel = new EmeraldLabel
        {
            Text = "ЗАГРУЗКА МАГАЗИНА...",
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center
        };
        container.AddChild(loadingLabel);

        _shopItemsPanel.AddChild(container);
    }

    public void ApplyState(DonateShopState state)
    {
        _state = state;

        if (state.HasError)
        {
            _topPanel.Visible = false;
            _tabsContainer.Visible = false;

            ShowError(state.ErrorMessage);
            return;
        }

        if (!state.IsRegistered)
        {
            _topPanel.Visible = false;
            _tabsContainer.Visible = false;

            ShowRegistrationRequired();
            return;
        }

        _topPanel.Visible = true;
        _tabsContainer.Visible = true;

        _levelBar.Level = state.Level;
        _levelBar.Experience = state.Experience;
        _levelBar.RequiredExp = state.RequiredExp;
        _levelBar.ToNextLevel = state.ToNextLevel;
        _levelBar.Progress = state.Progress;

        _batteryDisplay.Amount = (int)state.Energy;
        _crystalDisplay.Amount = state.Crystals;

        UpdateProfileContent(state);
        UpdateInventoryContent(state);
    }

    public void ApplyEnergyShopState(EnergyShopState state)
    {
        _energyShopState = state;
        _shopLoading = false;

        if (state.HasError)
        {
            ShowShopError(state.ErrorMessage);
            return;
        }

        UpdateShopContent(state);
    }

    public void ShowPurchaseResult(PurchaseResult result)
    {
        _isPurchasing = false;

        _shopItemsPanel.RemoveAllChildren();

        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            SeparationOverride = 16
        };

        var titleColor = result.Success ? Color.FromHex("#00FFAA") : Color.FromHex("#ff6b6b");
        var title = new EmeraldLabel
        {
            Text = result.Success ? "ПОКУПКА УСПЕШНА!" : "ОШИБКА ПОКУПКИ",
            TextColor = titleColor,
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center
        };
        container.AddChild(title);

        var message = new EmeraldLabel
        {
            Text = result.Message,
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center
        };
        container.AddChild(message);

        var backButton = new EmeraldButton
        {
            Text = "ВЕРНУТЬСЯ В МАГАЗИН",
            MinSize = new Vector2(220, 40),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };

        backButton.OnPressed += () =>
        {
            _energyShopState = null;
            _shopLoading = true;
            ShowShopLoading();
            _entManager.EntityNetManager.SendSystemNetworkMessage(new RequestEnergyShopItems());
        };
        container.AddChild(backButton);

        _shopItemsPanel.AddChild(container);
    }

    private void ShowShopError(string message)
    {
        _shopItemsPanel.RemoveAllChildren();

        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            SeparationOverride = 16
        };

        var title = new EmeraldLabel
        {
            Text = "ОШИБКА МАГАЗИНА",
            TextColor = Color.FromHex("#ff6b6b"),
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center
        };
        container.AddChild(title);

        var errorLabel = new EmeraldLabel
        {
            Text = message,
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center
        };
        container.AddChild(errorLabel);

        var retryButton = new EmeraldButton
        {
            Text = "ПОПРОБОВАТЬ СНОВА",
            MinSize = new Vector2(220, 40),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };

        retryButton.OnPressed += () =>
        {
            _shopLoading = true;
            ShowShopLoading();
            _entManager.EntityNetManager.SendSystemNetworkMessage(new RequestEnergyShopItems());
        };
        container.AddChild(retryButton);

        _shopItemsPanel.AddChild(container);
    }

    private void ShowRegistrationRequired()
    {
        _profilePanel.RemoveAllChildren();

        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            SeparationOverride = 16
        };

        var title = new EmeraldLabel
        {
            Text = "ТРЕБУЕТСЯ РЕГИСТРАЦИЯ",
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4)
        };
        container.AddChild(title);

        var message = new EmeraldLabel
        {
            Text = "Перейдите по ссылке и зарегистрируйтесь в веб ресурсе.\nДля регистрации может потребоваться VPN.",
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        container.AddChild(message);

        var button = new EmeraldButton
        {
            Text = "ПЕРЕЙТИ НА САЙТ",
            MinSize = new Vector2(200, 40),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };

        button.OnPressed += () =>
        {
            _url.OpenUri("https://deadspace14.net");
        };
        container.AddChild(button);

        _profilePanel.AddChild(container);
    }

    private void ShowError(string message)
    {
        _profilePanel.RemoveAllChildren();

        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            SeparationOverride = 16
        };

        var title = new EmeraldLabel
        {
            Text = "ОШИБКА",
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4)
        };
        container.AddChild(title);

        var errorLabel = new EmeraldLabel
        {
            Text = message,
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        container.AddChild(errorLabel);

        var retryButton = new EmeraldButton
        {
            Text = "ПОПРОБОВАТЬ СНОВА",
            MinSize = new Vector2(220, 40),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };

        retryButton.OnPressed += () =>
        {
            ShowLoading();
            _entManager.EntityNetManager.SendSystemNetworkMessage(new RequestUpdateDonateShop());
        };
        container.AddChild(retryButton);

        _profilePanel.AddChild(container);
    }

    private void UpdateProfileContent(DonateShopState state)
    {
        _profilePanel.RemoveAllChildren();

        var profileCard = new EmeraldProfileCard
        {
            PlayerName = state.PlayerUserName.ToUpper(),
            PlayerId = $"ID {state.Ss14PlayerId}",
            HorizontalExpand = true
        };
        _profilePanel.AddChild(profileCard);

        _perksGrid = new GridContainer
        {
            Columns = CalculatePerkColumns(),
            HorizontalExpand = true,
            HSeparationOverride = 8,
            VSeparationOverride = 8
        };

        var oocColor = state.OocColor.StartsWith("#") ? state.OocColor : "#" + state.OocColor;

        var oocCard = new EmeraldPerkCard
        {
            Title = "ЦВЕТ OOC",
            Value = oocColor,
            ValueColor = Color.FromHex(oocColor)
        };
        _perksGrid.AddChild(oocCard);

        var slotsCard = new EmeraldPerkCard
        {
            Title = "СЛОТЫ ПЕРСОНАЖЕЙ",
            Value = $"+{state.ExtraSlots}",
            ValueColor = Color.FromHex("#a589c9")
        };
        _perksGrid.AddChild(slotsCard);

        var priorityCard = new EmeraldPerkCard
        {
            Title = "ПРИОРИТЕТ ВХОДА",
            Value = state.HavePriorityJoinGame ? "ДА" : "НЕТ",
            ValueColor = state.HavePriorityJoinGame ? Color.FromHex("#a589c9") : Color.FromHex("#6d5a8a")
        };
        _perksGrid.AddChild(priorityCard);

        var antagCard = new EmeraldPerkCard
        {
            Title = "ПРИОРИТЕТ АНТАГА",
            Value = state.HavePriorityAntageGame ? "ДА" : "НЕТ",
            ValueColor = state.HavePriorityAntageGame ? Color.FromHex("#a589c9") : Color.FromHex("#6d5a8a")
        };
        _perksGrid.AddChild(antagCard);

        _profilePanel.AddChild(_perksGrid);

        if (state.CurrentPremium != null)
        {
            var premiumCard = new EmeraldPremiumCard
            {
                IsActive = state.CurrentPremium.Active,
                Level = state.CurrentPremium.PremiumLevel.Level,
                PremName = state.CurrentPremium.PremiumLevel.Name.ToUpper(),
                Description = state.CurrentPremium.PremiumLevel.Description,
                BonusXp = state.CurrentPremium.PremiumLevel.BonusXp,
                BonusEnergy = state.CurrentPremium.PremiumLevel.BonusEnergy,
                BonusSlots = state.CurrentPremium.PremiumLevel.BonusSlots,
                ExpiresIn = state.CurrentPremium.ExpiresIn,
                HorizontalExpand = true
            };
            _profilePanel.AddChild(premiumCard);
        }
        else
        {
            var buyPremiumCard = new EmeraldBuyPremiumCard
            {
                HorizontalExpand = true
            };
            buyPremiumCard.OnBuyPressed += () =>
            {
                _url.OpenUri("https://deadspace14.net");
            };
            _profilePanel.AddChild(buyPremiumCard);
        }

        if (state.Subscribes.Count > 0)
        {
            var subsContainer = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                HorizontalExpand = true,
                SeparationOverride = 8
            };

            foreach (var sub in state.Subscribes)
            {
                var isAdmin = sub.SubscribeName.StartsWith("[ADMIN]");
                var subCard = new EmeraldSubscriptionCard
                {
                    NameSub = sub.SubscribeName.ToUpper(),
                    Price = isAdmin ? "БЕСПЛАТНО" : $"{sub.Price} РУБ",
                    Dates = isAdmin ? "Навсегда" : $"Дата окончания: {sub.FinishDate}",
                    ItemCount = sub.Items.Count,
                    IsAdmin = isAdmin,
                    HorizontalExpand = true
                };
                subsContainer.AddChild(subCard);
            }

            _profilePanel.AddChild(subsContainer);
        }
        else
        {
            var buySubCard = new EmeraldBuySubscriptionCard
            {
                HorizontalExpand = true
            };
            buySubCard.OnBuyPressed += () =>
            {
                _url.OpenUri("https://deadspace14.net");
            };
            _profilePanel.AddChild(buySubCard);
        }
    }

    private List<DonateItemData> GetAllItems(DonateShopState state)
    {
        var allItems = new List<DonateItemData>(state.Items);

        foreach (var sub in state.Subscribes)
        {
            foreach (var subItem in sub.Items)
            {
                if (allItems.All(i => i.ItemIdInGame != subItem.ItemIdInGame || subItem.ItemIdInGame == null))
                {
                    allItems.Add(subItem);
                }
            }
        }

        return allItems;
    }

    private void UpdateInventoryContent(DonateShopState state)
    {
        _categoryTabsContainer.RemoveAllChildren();
        _categoryItemsPanel.RemoveAllChildren();

        var allItems = GetAllItems(state);

        if (allItems.Count == 0)
        {
            var emptyLabel = new EmeraldLabel
            {
                Text = "НЕТ ПРЕДМЕТОВ",
                TextColor = Color.FromHex("#6d5a8a"),
                Alignment = EmeraldLabel.TextAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            _categoryItemsPanel.AddChild(emptyLabel);
            return;
        }

        _categories = allItems
            .Select(i => i.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        foreach (var category in _categories)
        {
            var categoryTab = new EmeraldCategoryTab
            {
                Text = category.ToUpper()
            };

            var capturedCategory = category;
            categoryTab.OnPressed += () => SwitchCategory(capturedCategory);
            _categoryTabsContainer.AddChild(categoryTab);
        }

        var targetCategory = _currentCategory != null && _categories.Contains(_currentCategory)
            ? _currentCategory
            : _categories[0];

        SwitchCategory(targetCategory);
    }

    private void UpdateShopContent(EnergyShopState state)
    {
        _shopCategoryTabsContainer.RemoveAllChildren();
        _shopItemsPanel.RemoveAllChildren();

        if (state.Items.Count == 0)
        {
            var emptyLabel = new EmeraldLabel
            {
                Text = "НЕТ ТОВАРОВ В МАГАЗИНЕ",
                TextColor = Color.FromHex("#6d5a8a"),
                Alignment = EmeraldLabel.TextAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            _shopItemsPanel.AddChild(emptyLabel);
            return;
        }

        _shopCategories = state.Items
            .Select(i => i.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        foreach (var category in _shopCategories)
        {
            var categoryTab = new EmeraldCategoryTab
            {
                Text = category.ToUpper()
            };

            var capturedCategory = category;
            categoryTab.OnPressed += () => SwitchShopCategory(capturedCategory);
            _shopCategoryTabsContainer.AddChild(categoryTab);
        }

        var targetCategory = _currentShopCategory != null && _shopCategories.Contains(_currentShopCategory)
            ? _currentShopCategory
            : _shopCategories[0];

        SwitchShopCategory(targetCategory);
    }

    private void SwitchCategory(string category)
    {
        _currentCategory = category;
        _categoryItemsPanel.RemoveAllChildren();

        foreach (var child in _categoryTabsContainer.Children)
        {
            if (child is EmeraldCategoryTab tab)
            {
                tab.IsActive = tab.Text == category.ToUpper();
            }
        }

        if (_state == null)
            return;

        var allItems = GetAllItems(_state);

        var categoryItems = allItems
            .Where(i => i.Category == category)
            .Where(i => string.IsNullOrEmpty(_searchQuery) || i.ItemName.ToLower().Contains(_searchQuery))
            .ToList();

        if (categoryItems.Count == 0)
        {
            var emptyLabel = new EmeraldLabel
            {
                Text = "НЕТ ПРЕДМЕТОВ В ЭТОЙ КАТЕГОРИИ",
                TextColor = Color.FromHex("#6d5a8a"),
                Alignment = EmeraldLabel.TextAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            _categoryItemsPanel.AddChild(emptyLabel);
            return;
        }

        _itemsGrid = new GridContainer
        {
            Columns = CalculateColumns(),
            HorizontalExpand = true,
            VSeparationOverride = 8,
            HSeparationOverride = 8
        };

        foreach (var item in categoryItems)
        {
            var itemCard = new EmeraldItemCard
            {
                ItemName = item.ItemName.ToUpper(),
                ProtoId = item.ItemIdInGame ?? "",
                TimeFinish = item.TimeFinish,
                TimeAllways = item.TimeAllways,
                IsActive = item.IsActive,
                IsSpawned = _state.SpawnedItems.Contains(item.ItemIdInGame ?? ""),
                IsTimeUp = _state.IsTimeUp,
                SourceSubscription = item.SourceSubscription
            };

            itemCard.OnSpawnRequest += protoId =>
            {
                _entManager.EntityNetManager.SendSystemNetworkMessage(new DonateShopSpawnEvent(protoId));
            };

            _itemsGrid.AddChild(itemCard);
        }

        _categoryItemsPanel.AddChild(_itemsGrid);
    }

    private void SwitchShopCategory(string category)
    {
        _currentShopCategory = category;
        _shopItemsPanel.RemoveAllChildren();

        foreach (var child in _shopCategoryTabsContainer.Children)
        {
            if (child is EmeraldCategoryTab tab)
            {
                tab.IsActive = tab.Text == category.ToUpper();
            }
        }

        if (_energyShopState == null)
            return;

        var ownedItemIds = new HashSet<string>();
        if (_state != null)
        {
            foreach (var item in _state.Items)
            {
                if (!string.IsNullOrEmpty(item.ItemIdInGame))
                    ownedItemIds.Add(item.ItemIdInGame);
            }

            foreach (var sub in _state.Subscribes)
            {
                foreach (var subItem in sub.Items)
                {
                    if (!string.IsNullOrEmpty(subItem.ItemIdInGame))
                        ownedItemIds.Add(subItem.ItemIdInGame);
                }
            }
        }

        var categoryItems = _energyShopState.Items
            .Where(i => i.Category == category)
            .Where(i => string.IsNullOrEmpty(_shopSearchQuery) || i.Name.ToLower().Contains(_shopSearchQuery))
            .Where(i => !i.Owned && !ownedItemIds.Contains(i.ItemIdInGame))
            .ToList();

        if (categoryItems.Count == 0)
        {
            var emptyLabel = new EmeraldLabel
            {
                Text = "НЕТ ТОВАРОВ В ЭТОЙ КАТЕГОРИИ",
                TextColor = Color.FromHex("#6d5a8a"),
                Alignment = EmeraldLabel.TextAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            _shopItemsPanel.AddChild(emptyLabel);
            return;
        }

        _shopItemsGrid = new GridContainer
        {
            Columns = CalculateShopColumns(),
            HorizontalExpand = true,
            VSeparationOverride = 8,
            HSeparationOverride = 8
        };

        foreach (var item in categoryItems)
        {
            var itemCard = new EmeraldShopItemCard
            {
                ItemName = item.Name.ToUpper(),
                ItemId = item.Id,
                ProtoId = item.ItemIdInGame,
                Prices = item.Prices,
                Owned = item.Owned
            };

            itemCard.OnPurchaseRequest += (itemId, period) =>
            {
                if (_isPurchasing)
                    return;

                _isPurchasing = true;
                ShowPurchaseProcessing();
                _entManager.EntityNetManager.SendSystemNetworkMessage(new RequestPurchaseEnergyItem(itemId, period));
            };

            _shopItemsGrid.AddChild(itemCard);
        }

        _shopItemsPanel.AddChild(_shopItemsGrid);
    }

    private void ShowPurchaseProcessing()
    {
        _shopItemsPanel.RemoveAllChildren();

        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            SeparationOverride = 12
        };

        var loadingLabel = new EmeraldLabel
        {
            Text = "ОБРАБОТКА ПОКУПКИ...",
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center
        };
        container.AddChild(loadingLabel);

        var waitLabel = new EmeraldLabel
        {
            Text = "Подождите, пожалуйста",
            TextColor = Color.FromHex("#6d5a8a"),
            HorizontalAlignment = HAlignment.Center,
            Alignment = EmeraldLabel.TextAlignment.Center
        };
        container.AddChild(waitLabel);

        _shopItemsPanel.AddChild(container);
    }

    private int CalculateColumns()
    {
        const float itemWidth = 145f;
        const float spacing = 8f;
        const float padding = 20f;

        var availableWidth = Size.X - padding;
        var columns = (int)((availableWidth + spacing) / (itemWidth + spacing));

        return Math.Max(1, columns);
    }

    private int CalculateShopColumns()
    {
        const float itemWidth = 160f;
        const float spacing = 8f;
        const float padding = 20f;

        var availableWidth = Size.X - padding;
        var columns = (int)((availableWidth + spacing) / (itemWidth + spacing));

        return Math.Max(1, columns);
    }

    private int CalculatePerkColumns()
    {
        const float perkWidth = 140f;
        const float spacing = 8f;
        const float padding = 20f;

        var availableWidth = Size.X - padding;
        var columns = (int)((availableWidth + spacing) / (perkWidth + spacing));

        return Math.Max(1, columns);
    }

    protected override void Resized()
    {
        base.Resized();

        if (_itemsGrid != null)
        {
            _itemsGrid.Columns = CalculateColumns();
        }

        if (_shopItemsGrid != null)
        {
            _shopItemsGrid.Columns = CalculateShopColumns();
        }

        if (_perksGrid != null)
        {
            _perksGrid.Columns = CalculatePerkColumns();
        }
    }
}
