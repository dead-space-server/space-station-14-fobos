using System.Linq;
using System.Numerics;
using Content.Shared.DeadSpace.Arena;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Input;

namespace Content.Client.DeadSpace.Arena;

public sealed class ArenaLoadoutWindow : DefaultWindow
{
    public event Action<int>? OnLoadoutConfirmed;
    public event Action<List<string>>? OnStorePurchaseConfirm;

    private int _weaponSelection = -1;
    private ArenaWeaponCard? _selectedCard;
    private readonly BoxContainer _categoriesContainer;
    private readonly Button _confirmButton;

    private readonly TabContainer _tabContainer;
    private readonly BoxContainer _presetsTab;
    private readonly BoxContainer _storeTab;

    // Store tab state
    private readonly HashSet<string> _storeSelection = new();
    private readonly Label _storeBalanceLabel;
    private readonly Button _storeSaveButton;
    private readonly LineEdit _storeSearchInput;
    private string _storeSearchText = string.Empty;
    private List<ArenaTdmListingData> _storeListings = new();
    private List<ArenaTdmListingData> _filteredListings = new();
    private readonly Dictionary<string, PanelContainer> _storeRows = new();
    private readonly BoxContainer _storeContent;
    private bool _storeDirty = true;
    private int _maxBalance = 40;

    public ArenaLoadoutWindow()
    {
        Title = Loc.GetString("arena-loadout-title");
        MinSize = new Vector2(650, 550);
        SetSize = new Vector2(650, 550);

        _tabContainer = new TabContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        // === Tab 0: Presets ===
        _presetsTab = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        var subtitle = new Label
        {
            Text = Loc.GetString("arena-loadout-subtitle"),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6),
        };
        _presetsTab.AddChild(subtitle);

        _categoriesContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SeparationOverride = 4,
        };

        var presetsScroll = new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            Margin = new Thickness(6, 0),
        };
        presetsScroll.AddChild(_categoriesContainer);
        _presetsTab.AddChild(presetsScroll);

        _confirmButton = new Button
        {
            Text = Loc.GetString("arena-loadout-confirm"),
            Disabled = true,
            Margin = new Thickness(8, 6),
        };
        _confirmButton.OnPressed += _ =>
        {
            if (_weaponSelection >= 0)
                OnLoadoutConfirmed?.Invoke(_weaponSelection);
        };
        _presetsTab.AddChild(_confirmButton);

        TabContainer.SetTabTitle(_presetsTab, Loc.GetString("arena-tab-presets"));
        _tabContainer.AddChild(_presetsTab);

        // === Tab 1: TDM Store ===
        _storeTab = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        var storeCaption = new Label
        {
            Text = Loc.GetString("arena-store-title", ("balance", 40)),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4),
        };
        _storeTab.AddChild(storeCaption);

        _storeBalanceLabel = new Label
        {
            Text = Loc.GetString("arena-store-balance", ("remaining", 40)),
            HorizontalAlignment = HAlignment.Center,
            FontColorOverride = new Color(0.3f, 0.9f, 0.3f),
            Margin = new Thickness(0, 0, 0, 4),
        };
        _storeTab.AddChild(_storeBalanceLabel);

        _storeSearchInput = new LineEdit
        {
            PlaceHolder = Loc.GetString("arena-store-search-placeholder"),
            HorizontalExpand = true,
            Margin = new Thickness(6, 0, 6, 4),
        };
        _storeSearchInput.OnTextChanged += args =>
        {
            _storeSearchText = args.Text;
            _storeDirty = true;
            RebuildStoreTab();
        };
        _storeTab.AddChild(_storeSearchInput);

        _storeContent = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(6, 0),
        };
        var storeScroll = new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
        };
        storeScroll.AddChild(_storeContent);
        _storeTab.AddChild(storeScroll);

        _storeSaveButton = new Button
        {
            Text = Loc.GetString("arena-store-save"),
            Disabled = true,
            Margin = new Thickness(8, 6),
        };
        _storeSaveButton.OnPressed += _ =>
        {
            var ids = _storeSelection.ToList();
            OnStorePurchaseConfirm?.Invoke(ids);
        };
        _storeTab.AddChild(_storeSaveButton);

        TabContainer.SetTabTitle(_storeTab, Loc.GetString("arena-tab-store"));
        _tabContainer.AddChild(_storeTab);

        Contents.AddChild(_tabContainer);
    }

    public void UpdateState(ArenaLoadoutEuiState state)
    {
        // Tab 0: Presets
        _categoriesContainer.RemoveAllChildren();
        _selectedCard = null;
        _weaponSelection = -1;
        _confirmButton.Disabled = true;

        var categories = new List<(string Category, List<ArenaLoadoutOption> Options)>();
        var categoryMap = new Dictionary<string, List<ArenaLoadoutOption>>();

        foreach (var option in state.Weapons)
        {
            var category = Loc.GetString(option.Category);
            if (!categoryMap.TryGetValue(category, out var list))
            {
                list = new List<ArenaLoadoutOption>();
                categoryMap[category] = list;
                categories.Add((category, list));
            }
            list.Add(option);
        }

        foreach (var (category, options) in categories)
        {
            var header = new Label
            {
                Text = category,
                Margin = new Thickness(4, 6, 0, 2),
            };
            _categoriesContainer.AddChild(header);

            foreach (var option in options)
            {
                var card = new ArenaWeaponCard(
                    option.Index,
                    Loc.GetString(option.Name),
                    option.SpritePrototype,
                    Loc.GetString(option.Description));
                card.OnSelected += OnCardSelected;
                _categoriesContainer.AddChild(card);
            }
        }

        // Tab 1: TDM Store
        _storeTab.Visible = true;
        var prevListings = _storeListings;
        _storeListings = state.TdmStoreListings;
        _maxBalance = 40;

        // Only full rebuild if the listing set changed (different IDs or count)
        var listingChanged = prevListings.Count != _storeListings.Count ||
                             _storeListings.Any(l => !prevListings.Any(p => p.Id == l.Id));
        _storeDirty = _storeRows.Count == 0 || listingChanged;

        if (!_storeDirty)
        {
            // Sync server-side purchases with local selection
            var serverPurchased = new HashSet<string>(state.TdmPurchasedItems);
            foreach (var listing in _storeListings)
            {
                if (serverPurchased.Contains(listing.Id))
                    _storeSelection.Add(listing.Id);
                else
                    _storeSelection.Remove(listing.Id);
            }
        }
        else
        {
            _storeSelection.Clear();
            foreach (var id in state.TdmPurchasedItems)
                _storeSelection.Add(id);
        }

        RebuildStoreTab();
    }

    private void RebuildStoreTab()
    {
        if (!_storeDirty && _storeRows.Count > 0)
        {
            UpdateStoreBalance();
            return;
        }

        _storeContent.RemoveAllChildren();
        _storeRows.Clear();

        // Filter by search text
        _filteredListings = _storeListings;
        if (!string.IsNullOrWhiteSpace(_storeSearchText))
        {
            var query = _storeSearchText.ToLowerInvariant();
            _filteredListings = _storeListings
                .Where(l => Loc.GetString(l.Name).ToLowerInvariant().Contains(query))
                .ToList();
        }

        var categoryMap = new Dictionary<string, List<ArenaTdmListingData>>();
        foreach (var listing in _filteredListings)
        {
            var cat = Loc.GetString(listing.Category);
            if (!categoryMap.TryGetValue(cat, out var list))
            {
                list = new List<ArenaTdmListingData>();
                categoryMap[cat] = list;
            }
            list.Add(listing);
        }

        foreach (var (cat, items) in categoryMap)
        {
            var header = new Label
            {
                Text = cat,
                Margin = new Thickness(4, 6, 0, 2),
                FontColorOverride = new Color(0.8f, 0.8f, 1.0f),
            };
            _storeContent.AddChild(header);

            foreach (var item in items)
            {
                var row = BuildStoreItemRow(item);
                _storeContent.AddChild(row);
            }
        }

        _storeDirty = false;
        UpdateStoreBalance();
    }

    private static readonly StyleBoxFlat _defaultRowStyle = new()
    {
        BackgroundColor = new Color(0.12f, 0.12f, 0.14f),
        BorderColor = new Color(0.2f, 0.2f, 0.25f),
        BorderThickness = new Thickness(1, 1, 1, 1),
    };

    private static readonly StyleBoxFlat _selectedRowStyle = new()
    {
        BackgroundColor = new Color(0.15f, 0.3f, 0.15f),
        BorderColor = new Color(0.3f, 0.9f, 0.3f),
        BorderThickness = new Thickness(2, 2, 2, 2),
    };

    private Control BuildStoreItemRow(ArenaTdmListingData item)
    {
        var row = new PanelContainer
        {
            MinHeight = 34,
            HorizontalExpand = true,
            Margin = new Thickness(0, 1),
            MouseFilter = MouseFilterMode.Stop,
        };
        row.PanelOverride = _storeSelection.Contains(item.Id) ? _selectedRowStyle : _defaultRowStyle;

        var hbox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        // Item name
        var nameLabel = new Label
        {
            Text = Loc.GetString(item.Name),
            HorizontalExpand = true,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
        };
        hbox.AddChild(nameLabel);

        // Cost
        var costLabel = new Label
        {
            Text = Loc.GetString("arena-store-cost-format", ("cost", item.Cost)),
            MinSize = new Vector2(48, 28),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            FontColorOverride = new Color(0.3f, 0.9f, 0.3f),
        };
        hbox.AddChild(costLabel);

        row.AddChild(hbox);

        // Click to toggle
        row.OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            if (_storeSelection.Contains(item.Id))
            {
                _storeSelection.Remove(item.Id);
                row.PanelOverride = _defaultRowStyle;
            }
            else
            {
                var spent = GetCurrentSpent();
                if (spent + item.Cost > _maxBalance)
                    return;
                _storeSelection.Add(item.Id);
                row.PanelOverride = _selectedRowStyle;
            }
            UpdateStoreBalance();
            args.Handle();
        };

        _storeRows[item.Id] = row;
        return row;
    }

    private int GetCurrentSpent()
    {
        // Sum across ALL listings, not just filtered, so search doesn't affect balance
        return _storeListings
            .Where(l => _storeSelection.Contains(l.Id))
            .Sum(l => l.Cost);
    }

    private void UpdateStoreBalance()
    {
        var spent = GetCurrentSpent();
        var remaining = _maxBalance - spent;
        _storeBalanceLabel.Text = Loc.GetString("arena-store-balance", ("remaining", remaining));
        _storeBalanceLabel.FontColorOverride = remaining >= 0
            ? new Color(0.3f, 0.9f, 0.3f)
            : new Color(0.9f, 0.3f, 0.3f);
        _storeSaveButton.Disabled = _storeSelection.Count == 0 || remaining < 0;
    }

    private void OnCardSelected(ArenaWeaponCard card)
    {
        _selectedCard?.SetSelected(false);
        _selectedCard = card;
        _weaponSelection = card.WeaponIndex;
        card.SetSelected(true);
        _confirmButton.Disabled = false;
    }

    private sealed class ArenaWeaponCard : PanelContainer
    {
        public event Action<ArenaWeaponCard>? OnSelected;
        public int WeaponIndex { get; }

        private bool _isSelected;
        private static readonly StyleBoxFlat _selectedStyle = new()
        {
            BackgroundColor = new Color(0.2f, 0.45f, 0.2f),
            BorderColor = new Color(0.3f, 0.9f, 0.3f),
            BorderThickness = new Thickness(2, 2, 2, 2),
        };
        private static readonly StyleBoxFlat _defaultStyle = new()
        {
            BackgroundColor = new Color(0.1f, 0.1f, 0.12f),
            BorderColor = new Color(0.2f, 0.2f, 0.25f),
            BorderThickness = new Thickness(1, 1, 1, 1),
        };

        public ArenaWeaponCard(int weaponIndex, string weaponName, string? spritePrototype, string? tooltip = null)
        {
            WeaponIndex = weaponIndex;
            MouseFilter = MouseFilterMode.Stop;
            MinHeight = 56;
            HorizontalExpand = true;

            PanelOverride = _defaultStyle;

            if (!string.IsNullOrEmpty(tooltip))
            {
                ToolTip = tooltip;
            }

            var hbox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                HorizontalExpand = true,
                VerticalExpand = true,
            };

            if (!string.IsNullOrEmpty(spritePrototype))
            {
                var spriteView = new EntityPrototypeView
                {
                    MinSize = new Vector2(48, 48),
                    SetSize = new Vector2(48, 48),
                    HorizontalAlignment = HAlignment.Center,
                    VerticalAlignment = VAlignment.Center,
                    OverrideDirection = Direction.South,
                };
                spriteView.SetPrototype(spritePrototype);
                hbox.AddChild(spriteView);
            }

            var nameLabel = new Label
            {
                Text = weaponName,
                VerticalAlignment = VAlignment.Center,
                HorizontalExpand = true,
                Margin = new Thickness(8, 0, 0, 0),
            };
            hbox.AddChild(nameLabel);
            AddChild(hbox);

            OnKeyBindDown += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick)
                    return;
                OnSelected?.Invoke(this);
                args.Handle();
            };
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            PanelOverride = selected ? _selectedStyle : _defaultStyle;
        }
    }
}
